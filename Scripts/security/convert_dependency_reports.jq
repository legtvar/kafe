# Convert Trivy and NuGet Audit JSON into GitLab Dependency Scanning schema 15.2.4.

def severity_rank:
  {
    "UNKNOWN": 0,
    "INFO": 1,
    "LOW": 2,
    "MODERATE": 3,
    "MEDIUM": 3,
    "HIGH": 4,
    "CRITICAL": 5
  }[. // "UNKNOWN" | ascii_upcase] // 0;

def severity_name:
  {
    "UNKNOWN": "Unknown",
    "INFO": "Info",
    "LOW": "Low",
    "MODERATE": "Medium",
    "MEDIUM": "Medium",
    "HIGH": "High",
    "CRITICAL": "Critical"
  }[. // "UNKNOWN" | ascii_upcase] // "Unknown";

def clean_path:
  gsub("\\\\"; "/")
  | ltrimstr("./")
  | if startswith("/") or (split("/") | index("..")) then
      error("Dependency file must be a relative path inside the project: \(.)")
    else . end;

def advisory($value; $url):
  ($value | ascii_upcase) as $upper
  | if ($upper | test("^CVE-[0-9]{4}-[0-9]{4,}$")) then
      {type: "cve", name: $upper, value: $upper,
       url: ($url // "https://nvd.nist.gov/vuln/detail/\($upper)")}
    elif ($upper | test("^GHSA-[0-9A-Z-]+$")) then
      {type: "ghsa", name: $upper, value: $upper,
       url: "https://github.com/advisories/\($upper)"}
    else
      ({type: "trivy", name: $value, value: $value}
       + if $url then {url: $url} else {} end)
    end;

def new_finding($file; $ecosystem; $package; $version; $severity):
  {
    file: ($file | clean_path),
    ecosystem: $ecosystem,
    package: $package,
    version: $version,
    severity: ($severity | severity_name),
    identifiers: [],
    cwe_ids: [],
    links: [],
    title: "",
    description: "",
    fixed_version: "",
    direct: null
  };

def trivy_candidates:
  $trivy[0].Results as $results
  | if ($results | type) != "array" then error("Trivy output has no Results array") else . end
  | $results[]
  | . as $result
  | if (.Target // "" | length) == 0 then error("A Trivy result has no Target") else . end
  | (.Vulnerabilities // [])[]
  | . as $item
  | new_finding($result.Target; $result.Type; $item.PkgName;
                ($item.InstalledVersion // ""); $item.Severity)
  | .identifiers = ([$item.VulnerabilityID] + ($item.VendorIDs // [])
                    | map(select(. != null and . != "") | advisory(.; $item.PrimaryURL)))
  | if (.identifiers | length) == 0 then error("A Trivy vulnerability has no identifier") else . end
  | .title = ($item.Title // "Known vulnerability in \(.package)")
  | .description = ($item.Description // .title)
  | .fixed_version = ($item.FixedVersion // "")
  | .cwe_ids = ($item.CweIDs // [])
  | .links = ([$item.PrimaryURL] + ($item.References // [])
              | map(select(. != null and test("^https?://")) | {url: .}));

def nuget_lock_file($root):
  gsub("\\\\"; "/")
  | . as $path
  | if $path | startswith("/") then
      ($root | gsub("\\\\"; "/") | rtrimstr("/")) as $prefix
      | if $path | startswith($prefix + "/") then $path | ltrimstr($prefix + "/")
        else error("NuGet project is outside \($root): \($path)") end
    else $path end
  | split("/")
  | .[:-1]
  | join("/")
  | if . == "" then "packages.lock.json" else . + "/packages.lock.json" end
  | clean_path;

def nuget_candidates:
  $nuget[0].projects as $projects
  | if ($projects | type) != "array" then error("NuGet Audit output has no projects array") else . end
  | $projects[]
  | . as $project
  | ($project.frameworks // [])[]
  | . as $framework
  | (("topLevelPackages", "transitivePackages") as $section
     | ($framework[$section] // [])[]
     | . as $package
     | ($package.vulnerabilities // [])[]
     | . as $item
     | ($item.advisoryurl // $item.advisoryUrl) as $url
     | if $url == null then error("A NuGet vulnerability has no advisory URL") else . end
     | new_finding($project.path | nuget_lock_file($project_dir); "nuget";
                   $package.id; ($package.resolvedVersion // ""); $item.severity)
     | .title = "Known vulnerability in \(.package)"
     | .description = "NuGet Audit reported a vulnerability in \(.package) \(.version)."
     | .direct = ($section == "topLevelPackages")
     | .identifiers = [advisory($url | rtrimstr("/") | split("/") | last; $url)]
     | .links = [{$url} | {url: $url}]);

def advisory_keys:
  [.identifiers[] | select(.type != "cwe") | .type + ":" + (.value | ascii_upcase)];

def merge_findings($other):
  . as $current
  | if ($other.severity | severity_rank) > ($current.severity | severity_rank) then
      .severity = $other.severity
    else . end
  | if .title == "" then .title = $other.title else . end
  | if .description == "" then .description = $other.description else . end
  | if .fixed_version == "" then .fixed_version = $other.fixed_version else . end
  | .version = ([.version, $other.version] | map(select(. != "")) | unique | join(", "))
  | if $other.direct != null then .direct = ((.direct // false) or $other.direct) else . end
  | .identifiers = ((.identifiers + $other.identifiers) | unique_by(.type, .value))
  | .cwe_ids = ((.cwe_ids + $other.cwe_ids) | unique)
  | .links = ((.links + $other.links) | unique_by(.url));

def add_to_group($candidate):
  . as $group
  | ([range(0; $group | length) as $index
      | select((($group[$index] | advisory_keys) as $existing
                | ($candidate | advisory_keys) as $new
                | any($existing[]; . as $key | $new | index($key))))
      | $index] | first) as $match
  | if $match == null then . + [$candidate]
    else .[$match] = (.[$match] | merge_findings($candidate)) end;

def deduplicate:
  group_by(.file, (if .ecosystem == "nuget" or .ecosystem == "dotnet-core"
                   then .package | ascii_downcase else .package end))
  | map(reduce .[] as $candidate ([]; add_to_group($candidate)))
  | flatten;

def identifier_order:
  ({ghsa: 0, cve: 1}[.type] // 2), .value;

def gitlab_vulnerability:
  . as $finding
  | (.identifiers | sort_by(identifier_order)) as $advisories
  | ($advisories + ([.cwe_ids[]
                     | select(test("^CWE-[0-9]+$"))
                     | {type: "cwe", name: ., value: (split("-")[1]),
                        url: "https://cwe.mitre.org/data/definitions/\(split("-")[1]).html"}]))[:20]
  | . as $identifiers
  | ($identifiers[0].value) as $primary
  | {
      id: "kafe-third-party:\($primary):\($finding.file):\($finding.package):\($finding.version)",
      name: ($finding.title | .[0:255]),
      description: $finding.description,
      severity: $finding.severity,
      location: {
        file: $finding.file,
        dependency: ({
          package: {name: $finding.package},
          version: $finding.version
        } + if $finding.direct == null then {} else {direct: $finding.direct} end)
      },
      identifiers: $identifiers,
      links: ($finding.links | unique_by(.url)),
      solution: (if $finding.fixed_version != "" then
          "Upgrade \($finding.package) to one of these fixed versions: \($finding.fixed_version)."
        else "Review the linked advisory and upgrade to a non-vulnerable version." end)
    };

([trivy_candidates] + [nuget_candidates] | deduplicate) as $findings
| {
    version: "15.2.4",
    scan: {
      analyzer: {
        id: "kafe-third-party-dependency-scanner",
        name: "KAFE third-party dependency scanner",
        version: "1.0.0",
        vendor: {name: "KAFE"}
      },
      scanner: {
        id: "kafe-trivy-and-nuget-audit",
        name: "Trivy and NuGet Audit",
        version: "Trivy \($trivy_version) and NuGet Audit",
        vendor: {name: "Aqua Security and Microsoft"}
      },
      type: "dependency_scanning",
      start_time: $start_time,
      end_time: $end_time,
      status: "success"
    },
    vulnerabilities: ([$findings[] | gitlab_vulnerability]
                      | sort_by(-(.severity | severity_rank), .location.file,
                                (.location.dependency.package.name | ascii_downcase),
                                .identifiers[0].value))
  }
