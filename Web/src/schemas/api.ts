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
  "/api/v1/account/logout": {
    get: {
      responses: {
        /** @description Success */
        200: never;
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
            "text/plain": (components["schemas"]["AuthorListDto"])[];
            "application/json": (components["schemas"]["AuthorListDto"])[];
            "text/json": (components["schemas"]["AuthorListDto"])[];
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
            "text/plain": (components["schemas"]["PlaylistListDto"])[];
            "application/json": (components["schemas"]["PlaylistListDto"])[];
            "text/json": (components["schemas"]["PlaylistListDto"])[];
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
            "text/plain": (components["schemas"]["ProjectGroupListDto"])[];
            "application/json": (components["schemas"]["ProjectGroupListDto"])[];
            "text/json": (components["schemas"]["ProjectGroupListDto"])[];
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
            "text/plain": (components["schemas"]["ProjectListDto"])[];
            "application/json": (components["schemas"]["ProjectListDto"])[];
            "text/json": (components["schemas"]["ProjectListDto"])[];
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
        200: never;
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
  "/api/v1/tmp-account/{token}": {
    get: {
      parameters: {
        path: {
          token: string;
        };
      };
      responses: {
        /** @description Success */
        200: never;
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
        200: never;
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
      /** @description The projects this account is an owner of. */
      projects: (components["schemas"]["ProjectListDto"])[];
      /** @description The capabilities this user has been granted. */
      capabilities: (string)[];
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
      containingProject: string;
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
      visibility: components["schemas"]["Visibility"];
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
      id: string;
      name: string;
      visibility: components["schemas"]["Visibility"];
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
      id: string;
      name: string;
      visibility: components["schemas"]["Visibility"];
    };
    /** @enum {string} */
    DiagnosticKind: "Unknown" | "Info" | "Warning" | "Error";
    ImageDto: {
      fileExtension: string;
      formatName: string;
      mimeType: string;
      /** Format: int32 */
      width: number;
      /** Format: int32 */
      height: number;
      isCorrupted: boolean;
    };
    ImageShardDetailDto: WithRequired<({
      kind: "Image";
      variants: {
        [key: string]: components["schemas"]["ImageDto"] | undefined;
      };
    }) & Omit<components["schemas"]["ShardDetailBaseDto"], "kind">, "variants">;
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
      videoStreams: (components["schemas"]["VideoStreamDto"])[];
      audioStreams: (components["schemas"]["AudioStreamDto"])[];
      subtitleStreams: (components["schemas"]["SubtitleStreamDto"])[];
      isCorrupted: boolean;
    };
    PlaylistDetailDto: {
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
      visibility: components["schemas"]["Visibility"];
      videos: (string)[];
    };
    PlaylistListDto: {
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
      visibility: components["schemas"]["Visibility"];
    };
    ProblemDetails: {
      type?: string | null;
      title?: string | null;
      /** Format: int32 */
      status?: number | null;
      detail?: string | null;
      instance?: string | null;
      [key: string]: unknown | undefined;
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
      slotName: string;
      arity: components["schemas"]["ArgumentArity"];
      shardBlueprints: (components["schemas"]["ProjectArtifactShardBlueprintDto"])[];
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
      shards: (components["schemas"]["ShardListDto"])[];
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
      kind: components["schemas"]["ShardKind"];
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
      roles: (string)[];
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
      requiredReviewers: (string)[];
      artifactBlueprints: (components["schemas"]["ProjectArtifactBlueprintDto"])[];
    };
    ProjectCreationAuthorDto: {
      id: string;
      roles: (string)[];
    };
    ProjectCreationDto: {
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
      crew: (components["schemas"]["ProjectCreationAuthorDto"])[];
      cast: (components["schemas"]["ProjectCreationAuthorDto"])[];
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
      visibility: components["schemas"]["Visibility"];
      /** Format: date-time */
      releasedOn: string;
      crew: (components["schemas"]["ProjectAuthorDto"])[];
      cast: (components["schemas"]["ProjectAuthorDto"])[];
      artifacts: (components["schemas"]["ProjectArtifactDto"])[];
      reviews: (components["schemas"]["ProjectReviewDto"])[];
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
      projects: (components["schemas"]["ProjectListDto"])[];
    };
    ProjectGroupListDto: {
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
      visibility: components["schemas"]["Visibility"];
      /** Format: date-time */
      releasedOn: string;
    };
    ProjectReviewCreationDto: {
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
      diagnostics: (components["schemas"]["ProjectDiagnosticDto"])[];
    };
    /** @enum {string} */
    ReviewKind: "NotReviewed" | "Accepted" | "Rejected";
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
    ShardKind: "Unknown" | "Video" | "Image" | "Subtitles";
    ShardListDto: {
      /**
       * Format: hrib 
       * @description Human-Readable Identifier Ballast 
       * @example AAAAbadf00d
       */
      id: string;
      kind: components["schemas"]["ShardKind"];
      variants: (string)[];
    };
    SubtitleStreamDto: {
      codec: string;
      /** Format: int64 */
      bitrate: number;
    };
    TemporaryAccountCreationDto: {
      emailAddress: string;
      preferredCulture?: string | null;
    };
    VideoShardDetailDto: WithRequired<({
      kind: "Video";
      variants: {
        [key: string]: components["schemas"]["MediaDto"] | undefined;
      };
    }) & Omit<components["schemas"]["ShardDetailBaseDto"], "kind">, "variants">;
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
    /** @enum {string} */
    Visibility: "Unknown" | "Private" | "Internal" | "Public";
  };
  responses: never;
  parameters: never;
  requestBodies: never;
  headers: never;
  pathItems: never;
}

export type external = Record<string, never>;

export type operations = Record<string, never>;
