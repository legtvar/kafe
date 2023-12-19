/**
 * This file was auto-generated by openapi-typescript.
 * Do not make direct changes to the file.
 */


/** WithRequired type helpers */
type WithRequired<T, K extends keyof T> = T & { [P in K]-?: T[P] };

export interface paths {
  "/api/v1/account": {
    get: {
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["AccountDetailDto"];
            "application/json": components["schemas"]["AccountDetailDto"];
            "text/json": components["schemas"]["AccountDetailDto"];
          };
        };
        /** @description Bad Request */
        400: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
        /** @description Not Found */
        404: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
      };
    };
  };
  "/api/v1/account/{id}": {
    get: {
      parameters: {
        path: {
          id: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["AccountDetailDto"];
            "application/json": components["schemas"]["AccountDetailDto"];
            "text/json": components["schemas"]["AccountDetailDto"];
          };
        };
        /** @description Bad Request */
        400: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
        /** @description Not Found */
        404: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
      };
    };
  };
  "/api/v1/accounts": {
    get: {
      parameters: {
        query?: {
          AccessedEntityId?: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["AccountListDto"][];
            "application/json": components["schemas"]["AccountListDto"][];
            "text/json": components["schemas"]["AccountListDto"][];
          };
        };
      };
    };
  };
  "/api/v1/account/logout": {
    get: {
      responses: {
        /** @description Success */
        200: {
          content: never;
        };
      };
    };
  };
  "/api/v1/artifact": {
    post: {
      requestBody?: {
        content: {
          "application/json": components["schemas"]["ArtifactCreationDto"];
          "text/json": components["schemas"]["ArtifactCreationDto"];
          "application/*+json": components["schemas"]["ArtifactCreationDto"];
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": string;
            "application/json": string;
            "text/json": string;
          };
        };
        /** @description Bad Request */
        400: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
      };
    };
  };
  "/api/v1/artifact/{id}": {
    get: {
      parameters: {
        path: {
          id: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": string;
            "application/json": string;
            "text/json": string;
          };
        };
        /** @description Bad Request */
        400: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
      };
    };
  };
  "/api/v1/author": {
    post: {
      requestBody?: {
        content: {
          "application/json": components["schemas"]["AuthorCreationDto"];
          "text/json": components["schemas"]["AuthorCreationDto"];
          "application/*+json": components["schemas"]["AuthorCreationDto"];
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": string;
            "application/json": string;
            "text/json": string;
          };
        };
      };
    };
  };
  "/api/v1/author/{id}": {
    get: {
      parameters: {
        path: {
          id: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["AuthorDetailDto"];
            "application/json": components["schemas"]["AuthorDetailDto"];
            "text/json": components["schemas"]["AuthorDetailDto"];
          };
        };
        /** @description Not Found */
        404: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
      };
    };
  };
  "/api/v1/authors": {
    get: {
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["AuthorListDto"][];
            "application/json": components["schemas"]["AuthorListDto"][];
            "text/json": components["schemas"]["AuthorListDto"][];
          };
        };
      };
    };
  };
  "/api/v1/entity/perms/{id}": {
    get: {
      parameters: {
        path: {
          id: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["EntityPermissionsDetailDto"];
            "application/json": components["schemas"]["EntityPermissionsDetailDto"];
            "text/json": components["schemas"]["EntityPermissionsDetailDto"];
          };
        };
      };
    };
  };
  "/api/v1/entity/perms": {
    patch: {
      requestBody?: {
        content: {
          "application/json": components["schemas"]["EntityPermissionsEditDto"];
          "text/json": components["schemas"]["EntityPermissionsEditDto"];
          "application/*+json": components["schemas"]["EntityPermissionsEditDto"];
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": string;
            "application/json": string;
            "text/json": string;
          };
        };
      };
    };
  };
  "/api/v1/playlist/{id}": {
    get: {
      parameters: {
        path: {
          id: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["PlaylistDetailDto"];
            "application/json": components["schemas"]["PlaylistDetailDto"];
            "text/json": components["schemas"]["PlaylistDetailDto"];
          };
        };
        /** @description Not Found */
        404: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
      };
    };
  };
  "/api/v1/playlists": {
    get: {
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["PlaylistListDto"][];
            "application/json": components["schemas"]["PlaylistListDto"][];
            "text/json": components["schemas"]["PlaylistListDto"][];
          };
        };
      };
    };
  };
  "/api/v1/project": {
    post: {
      requestBody?: {
        content: {
          "application/json": components["schemas"]["ProjectCreationDto"];
          "text/json": components["schemas"]["ProjectCreationDto"];
          "application/*+json": components["schemas"]["ProjectCreationDto"];
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": string;
            "application/json": string;
            "text/json": string;
          };
        };
      };
    };
    patch: {
      requestBody?: {
        content: {
          "application/json": components["schemas"]["ProjectEditDto"];
          "text/json": components["schemas"]["ProjectEditDto"];
          "application/*+json": components["schemas"]["ProjectEditDto"];
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: never;
        };
      };
    };
  };
  "/api/v1/project/{id}": {
    get: {
      parameters: {
        path: {
          id: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["ProjectDetailDto"];
            "application/json": components["schemas"]["ProjectDetailDto"];
            "text/json": components["schemas"]["ProjectDetailDto"];
          };
        };
        /** @description Not Found */
        404: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
      };
    };
  };
  "/api/v1/project-group": {
    post: {
      requestBody?: {
        content: {
          "application/json": components["schemas"]["ProjectGroupCreationDto"];
          "text/json": components["schemas"]["ProjectGroupCreationDto"];
          "application/*+json": components["schemas"]["ProjectGroupCreationDto"];
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": string;
            "application/json": string;
            "text/json": string;
          };
        };
      };
    };
    patch: {
      requestBody?: {
        content: {
          "application/json": components["schemas"]["ProjectGroupEditDto"];
          "text/json": components["schemas"]["ProjectGroupEditDto"];
          "application/*+json": components["schemas"]["ProjectGroupEditDto"];
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: never;
        };
      };
    };
  };
  "/api/v1/project-group/{id}": {
    get: {
      parameters: {
        path: {
          id: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["ProjectGroupDetailDto"];
            "application/json": components["schemas"]["ProjectGroupDetailDto"];
            "text/json": components["schemas"]["ProjectGroupDetailDto"];
          };
        };
        /** @description Not Found */
        404: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
      };
    };
  };
  "/api/v1/project-groups": {
    get: {
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["ProjectGroupListDto"][];
            "application/json": components["schemas"]["ProjectGroupListDto"][];
            "text/json": components["schemas"]["ProjectGroupListDto"][];
          };
        };
      };
    };
  };
  "/api/v1/projects": {
    get: {
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["ProjectListDto"][];
            "application/json": components["schemas"]["ProjectListDto"][];
            "text/json": components["schemas"]["ProjectListDto"][];
          };
        };
      };
    };
  };
  "/api/v1/project-review": {
    post: {
      requestBody?: {
        content: {
          "application/json": components["schemas"]["ProjectReviewCreationDto"];
          "text/json": components["schemas"]["ProjectReviewCreationDto"];
          "application/*+json": components["schemas"]["ProjectReviewCreationDto"];
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: never;
        };
      };
    };
  };
  "/api/v1/project-validation/{id}": {
    get: {
      parameters: {
        path: {
          id: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["ProjectValidationDto"];
            "application/json": components["schemas"]["ProjectValidationDto"];
            "text/json": components["schemas"]["ProjectValidationDto"];
          };
        };
      };
    };
  };
  "/api/v1/shard": {
    post: {
      requestBody?: {
        content: {
          "multipart/form-data": {
            Kind?: components["schemas"]["ShardKind"];
            ArtifactId?: string;
            /** Format: binary */
            File?: string;
          };
        };
      };
      responses: {
        /** @description Forbidden */
        403: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
        /** @description Not Found */
        404: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
      };
    };
  };
  "/api/v1/shard/{id}": {
    get: {
      parameters: {
        path: {
          id: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["VideoShardDetailDto"] | components["schemas"]["ImageShardDetailDto"];
            "application/json": components["schemas"]["VideoShardDetailDto"] | components["schemas"]["ImageShardDetailDto"];
            "text/json": components["schemas"]["VideoShardDetailDto"] | components["schemas"]["ImageShardDetailDto"];
          };
        };
        /** @description Forbidden */
        403: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
        /** @description Not Found */
        404: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
      };
    };
  };
  "/api/v1/shard-download/{id}": {
    get: {
      parameters: {
        path: {
          Id: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": string;
            "application/json": string;
            "text/json": string;
          };
        };
      };
    };
  };
  "/api/v1/shard-download/{id}/{variant}": {
    get: {
      parameters: {
        path: {
          Id: string;
          Variant: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": string;
            "application/json": string;
            "text/json": string;
          };
        };
      };
    };
  };
  "/api/v1/system": {
    get: {
      responses: {
        /** @description Success */
        200: {
          content: {
            "text/plain": components["schemas"]["SystemDetailDto"];
            "application/json": components["schemas"]["SystemDetailDto"];
            "text/json": components["schemas"]["SystemDetailDto"];
          };
        };
      };
    };
  };
  "/api/v1/tmp-account/{token}": {
    get: {
      parameters: {
        path: {
          token: string;
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: never;
        };
        /** @description Bad Request */
        400: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
        /** @description Not Found */
        404: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
      };
    };
  };
  "/api/v1/tmp-account": {
    post: {
      requestBody?: {
        content: {
          "application/json": components["schemas"]["TemporaryAccountCreationDto"];
          "text/json": components["schemas"]["TemporaryAccountCreationDto"];
          "application/*+json": components["schemas"]["TemporaryAccountCreationDto"];
        };
      };
      responses: {
        /** @description Success */
        200: {
          content: never;
        };
        /** @description Bad Request */
        400: {
          content: {
            "text/plain": components["schemas"]["ProblemDetails"];
            "application/json": components["schemas"]["ProblemDetails"];
            "text/json": components["schemas"]["ProblemDetails"];
          };
        };
      };
    };
  };
}

export type webhooks = Record<string, never>;

export interface components {
  schemas: {
    /** @description A detail of a user account. */
    AccountDetailDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      /** @description The name of the user. Null if the account is temporary. */
      name?: string | null;
      /** @description The uco of the user. Null if the account is temporary. */
      uco?: string | null;
      /** @description The email address of the user. */
      emailAddress: string;
      /** @description The preferred culture of the user. */
      preferredCulture: string;
      /** @description The explicit permissions this user has been granted. */
      permissions: {
        [key: string]: components["schemas"]["Permission"][];
      };
    };
    AccountListDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      emailAddress: string;
      preferredCulture: string;
      permissions: {
        [key: string]: components["schemas"]["Permission"][];
      };
    };
    /** @description The lower and upper bound for the count of something. */
    ArgumentArity: {
      /**
       * Format: int32
       * @description The inclusive lower bound.
       */
      min: number;
      /**
       * Format: int32
       * @description The inclusive upper bound.
       */
      max: number;
    };
    ArtifactCreationDto: {
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** Format: date-time */
      addedOn?: string | null;
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      containingProject?: string | null;
      blueprintSlot?: string | null;
    };
    AudioStreamDto: {
      codec: string;
      /** Format: int64 */
      bitrate: number;
      /** Format: int32 */
      channels: number;
      /** Format: int32 */
      sampleRate: number;
    };
    AuthorCreationDto: {
      name: string;
      globalPermissions: components["schemas"]["Permission"];
      /** LocalizedString */
      bio?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      uco?: string | null;
      email?: string | null;
      phone?: string | null;
    };
    AuthorDetailDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      name: string;
      globalPermissions: components["schemas"]["Permission"];
      /** LocalizedString */
      bio?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      uco?: string | null;
      email?: string | null;
      phone?: string | null;
    };
    AuthorListDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      name: string;
      globalPermissions: components["schemas"]["Permission"];
    };
    /** @enum {string} */
    DiagnosticKind: "unknown" | "info" | "warning" | "error";
    EntityPermissionsAccountEditDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id?: string | null;
      emailAddress?: string | null;
      permissions: components["schemas"]["Permission"][];
    };
    EntityPermissionsAccountListDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      emailAddress: string;
      permissions: components["schemas"]["Permission"][];
    };
    EntityPermissionsDetailDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      entityType?: string | null;
      globalPermissions?: components["schemas"]["Permission"][] | null;
      userPermissions?: components["schemas"]["Permission"][] | null;
      accountPermissions: components["schemas"]["EntityPermissionsAccountListDto"][];
    };
    EntityPermissionsEditDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      globalPermissions?: components["schemas"]["Permission"][] | null;
      accountPermissions?: components["schemas"]["EntityPermissionsAccountEditDto"][] | null;
    };
    ImageDto: {
      fileExtension: string;
      mimeType: string;
      /** Format: int32 */
      width: number;
      /** Format: int32 */
      height: number;
      isCorrupted: boolean;
    };
    ImageShardDetailDto: WithRequired<{
      kind: "Image";
      variants: {
        [key: string]: components["schemas"]["ImageDto"];
      };
    } & Omit<components["schemas"]["ShardDetailBaseDto"], "kind">, "variants">;
    MediaDto: {
      fileExtension: string;
      mimeType: string;
      /** Format: int64 */
      fileLength: number;
      /**
       * Format: time-span
       * @example 00:00:00
       */
      duration: string;
      videoStreams: components["schemas"]["VideoStreamDto"][];
      audioStreams: components["schemas"]["AudioStreamDto"][];
      subtitleStreams: components["schemas"]["SubtitleStreamDto"][];
      isCorrupted: boolean;
      error?: string | null;
    };
    /** @enum {string} */
    Permission: "none" | "read" | "append" | "inspect" | "write" | "review" | "all";
    PlaylistDetailDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      globalPermissions: components["schemas"]["Permission"];
      videos: string[];
    };
    PlaylistListDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      globalPermissions: components["schemas"]["Permission"];
    };
    ProblemDetails: {
      type?: string | null;
      title?: string | null;
      /** Format: int32 */
      status?: number | null;
      detail?: string | null;
      instance?: string | null;
      [key: string]: unknown;
    };
    ProjectArtifactAdditionDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      blueprintSlot?: string | null;
    };
    ProjectArtifactBlueprintDto: {
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      arity: components["schemas"]["ArgumentArity"];
      shardBlueprints: {
        [key: string]: components["schemas"]["ProjectArtifactShardBlueprintDto"];
      };
    };
    ProjectArtifactDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** Format: date-time */
      addedOn: string;
      blueprintSlot?: string | null;
      shards: components["schemas"]["ShardListDto"][];
    };
    ProjectArtifactShardBlueprintDto: {
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      arity: components["schemas"]["ArgumentArity"];
    };
    ProjectAuthorDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      name: string;
      roles: string[];
    };
    ProjectBlueprintDto: {
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      requiredReviewers: string[];
      artifactBlueprints: {
        [key: string]: components["schemas"]["ProjectArtifactBlueprintDto"];
      };
    };
    ProjectCreationAuthorDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      roles: string[];
    };
    ProjectCreationDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      projectGroupId: string;
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      /** LocalizedString */
      genre?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      crew: components["schemas"]["ProjectCreationAuthorDto"][];
      cast: components["schemas"]["ProjectCreationAuthorDto"][];
    };
    ProjectDetailDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      projectGroupId: string;
      /** LocalizedString */
      projectGroupName?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      /** LocalizedString */
      genre?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      globalPermissions: components["schemas"]["Permission"][];
      userPermissions: components["schemas"]["Permission"][];
      /** Format: date-time */
      releasedOn: string;
      crew: components["schemas"]["ProjectAuthorDto"][];
      cast: components["schemas"]["ProjectAuthorDto"][];
      artifacts: components["schemas"]["ProjectArtifactDto"][];
      reviews: components["schemas"]["ProjectReviewDto"][];
      blueprint: components["schemas"]["ProjectBlueprintDto"];
    };
    ProjectDiagnosticDto: {
      kind: components["schemas"]["DiagnosticKind"];
      /** LocalizedString */
      message: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      validationStage: string;
    };
    ProjectEditDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      /** LocalizedString */
      name?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      /** LocalizedString */
      genre?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      crew?: components["schemas"]["ProjectCreationAuthorDto"][] | null;
      cast?: components["schemas"]["ProjectCreationAuthorDto"][] | null;
      artifacts?: components["schemas"]["ProjectArtifactAdditionDto"][] | null;
    };
    ProjectGroupCreationDto: {
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      /** Format: date-time */
      deadline: string;
    };
    ProjectGroupDetailDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      /** Format: date-time */
      deadline: string;
      isOpen: boolean;
      projects: components["schemas"]["ProjectListDto"][];
    };
    ProjectGroupEditDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      /** LocalizedString */
      name?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      /** Format: date-time */
      deadline?: string | null;
      isOpen?: boolean | null;
    };
    ProjectGroupListDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      /** Format: date-time */
      deadline: string;
      isOpen: boolean;
    };
    /** @description A DTO of a project. To be used when listing projects. */
    ProjectListDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      projectGroupId: string;
      /** LocalizedString */
      name: {
        iv: string;
        cs?: string | null;
        en?: string | null;
      };
      /** LocalizedString */
      description?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      /** @description Permissions that apply to all users, even the anonymous ones. */
      globalPermissions: components["schemas"]["Permission"][];
      /** @description Permissions that apply to the currently logged in user. Includes the global permissions. */
      userPermissions: components["schemas"]["Permission"][];
      /** Format: date-time */
      releasedOn: string;
    };
    ProjectReviewCreationDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      projectId: string;
      kind: components["schemas"]["ReviewKind"];
      reviewerRole: string;
      /** LocalizedString */
      comment?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
    };
    ProjectReviewDto: {
      kind: components["schemas"]["ReviewKind"];
      reviewerRole: string;
      /** LocalizedString */
      comment?: ({
        iv: string;
        cs?: string | null;
        en?: string | null;
      }) | null;
      /** Format: date-time */
      addedOn: string;
    };
    ProjectValidationDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      projectId: string;
      /** Format: date-time */
      validatedOn: string;
      diagnostics: components["schemas"]["ProjectDiagnosticDto"][];
    };
    /** @enum {string} */
    ReviewKind: "notReviewed" | "accepted" | "rejected";
    ShardDetailBaseDto: {
      kind: components["schemas"]["ShardKind"];
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      artifactId: string;
    };
    /** @enum {string} */
    ShardKind: "unknown" | "video" | "image" | "subtitles";
    ShardListDto: {
      /**
       * Format: hrib
       * @description Human-Readable Identifier Ballast
       * @example AAAAbadf00d
       */
      id: string;
      kind: components["schemas"]["ShardKind"];
      variants: string[];
    };
    SubtitleStreamDto: {
      codec: string;
      /** Format: int64 */
      bitrate: number;
    };
    SystemDetailDto: {
      name: string;
      baseUrls: string[];
      version: string;
      commit: string;
      /** Format: date-time */
      commitDate: string;
      /** Format: date-time */
      runningSince: string;
    };
    TemporaryAccountCreationDto: {
      emailAddress: string;
      preferredCulture?: string | null;
    };
    VideoShardDetailDto: WithRequired<{
      kind: "Video";
      variants: {
        [key: string]: components["schemas"]["MediaDto"];
      };
    } & Omit<components["schemas"]["ShardDetailBaseDto"], "kind">, "variants">;
    VideoStreamDto: {
      codec: string;
      /** Format: int64 */
      bitrate: number;
      /** Format: int32 */
      width: number;
      /** Format: int32 */
      height: number;
      /** Format: double */
      framerate: number;
    };
  };
  responses: never;
  parameters: never;
  requestBodies: never;
  headers: never;
  pathItems: never;
}

export type $defs = Record<string, never>;

export type external = Record<string, never>;

export type operations = Record<string, never>;
