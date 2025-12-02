import bpy
import argparse
import sys

class ArgumentParserForBlender(argparse.ArgumentParser):
    """
    This class is identical to its superclass, except for the parse_args
    method (see docstring). It resolves the ambiguity generated when calling
    Blender from the CLI with a python script, and both Blender and the script
    have arguments. E.g., the following call will make Blender crash because
    it will try to process the script's -a and -b flags:
    >>> blender --python my_script.py -a 1 -b 2

    To bypass this issue this class uses the fact that Blender will ignore all
    arguments given after a double-dash ('--'). The approach is that all
    arguments before '--' go to Blender, arguments after go to the script.
    The following calls work fine:
    >>> blender --python my_script.py -- -a 1 -b 2
    >>> blender --python my_script.py --
    """

    def _get_argv_after_doubledash(self):
        """
        Given the sys.argv as a list of strings, this method returns the
        sublist right after the '--' element (if present, otherwise returns
        an empty list).
        """
        try:
            idx = sys.argv.index("--")
            return sys.argv[idx + 1 :]  # the list after '--'
        except ValueError as e:  # '--' not in the list:
            return []

    # overrides superclass
    def parse_args(self):
        """
        This method is expected to behave identically as in the superclass,
        except that the sys.argv list will be pre-processed using
        _get_argv_after doubledash before. See the docstring of the class for
        usage examples and details.
        """
        return super().parse_args(args=self._get_argv_after_doubledash())

def install_extension(args):
    print(f"Installing extension {args.id} from repo {args.repo_name} ({args.repo_url})")
    repos = bpy.context.preferences.extensions.repos
    pigeons_repo = repos.get(args.repo_name)

    if args.allow_online:
        bpy.ops.extensions.userpref_allow_online()
    if bpy.context.preferences.system.use_online_access == False:
        raise ValueError("Online access is disabled in Blender preferences. Use --allow-online to override.")
    bpy.context.preferences.use_preferences_save = True
    if pigeons_repo is None:
        pigeons_repo = repos.new(name=args.repo_name, remote_url=args.repo_url)
    repo_index = repos.find(args.repo_name)
    
    
    bpy.ops.extensions.repo_sync(repo_directory=pigeons_repo.directory, repo_index=repo_index)
    bpy.ops.extensions.package_install(repo_index=repo_index, pkg_id=args.id)
    bpy.ops.wm.save_userpref()

    print(f"{args.id.capitalize()} updated successfully.")


parser = ArgumentParserForBlender(description="Update and install a Blender extension.")

def parse_args():
    parser.add_argument('--repo-name', type=str, help='Name of the extension repository.')
    parser.add_argument('--repo-url', type=str, help='URL of the extension repository.')
    parser.add_argument('--id', type=str, help='ID of the extension package.')
    parser.add_argument('--allow-online', action='store_true', help='Allow online access in Blender.')
    return parser.parse_args()

if __name__ == "__main__":
    args = parse_args()
    try:
        install_extension(args)
    except Exception as e:
        print(f"Failed to install extension: {e}")