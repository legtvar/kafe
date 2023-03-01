/**
 * This file was auto-generated by openapi-typescript.
 * Do not make direct changes to the file.
 */


export interface paths {
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
  "/api/v1/shard": {
    post: {
      requestBody?: {
        content: {
          "multipart/form-data": {
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
  "/api/v1/shard/{id}/{variant}": {
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
            "application/octet-stream": string;
          };
        };
      };
    };
  };
}

export type webhooks = Record<string, never>;

export interface components {
  schemas: {
    ArtifactCreationDto: {
      name: components["schemas"]["LocalizedString"];
      /** Format: hrib */
      containingProject?: string | null;
    };
    ArtifactDetailDto: {
      /** Format: hrib */
      id: string;
      name: components["schemas"]["LocalizedString"];
      shards: (components["schemas"]["ShardListDto"])[];
      containingProjectIds: (string)[];
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
    AuthorDetailDto: {
      id: string;
      name: string;
      uco?: string | null;
      email?: string | null;
      phone?: string | null;
    };
    AuthorListDto: {
      id: string;
      name: string;
    };
    ImageDto: {
      /** Format: int32 */
      width: number;
      /** Format: int32 */
      height: number;
      format: string;
    };
    ImageShardDetailDto: ({
      Kind: "ImageShardDetailDto";
      variants: {
        [key: string]: components["schemas"]["ImageDto"] | undefined;
      };
    }) & Omit<components["schemas"]["ShardDetailBaseDto"], "Kind">;
    LocalizedString: Record<string, never>;
    MediaDto: {
      duration: components["schemas"]["TimeSpan"];
      videoStreams: (components["schemas"]["VideoStreamDto"])[];
      audioStreams: (components["schemas"]["AudioStreamDto"])[];
      subtitleStreams: (components["schemas"]["SubtitleStreamDto"])[];
    };
    PlaylistDetailDto: {
      id: string;
      name: components["schemas"]["LocalizedString"];
      description: components["schemas"]["LocalizedString"];
      visibility: components["schemas"]["Visibility"];
      videos: (string)[];
    };
    PlaylistListDto: {
      id: string;
      name: components["schemas"]["LocalizedString"];
      description: components["schemas"]["LocalizedString"];
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
    ProjectAuthorDto: {
      id: string;
      name: string;
      roles: (string)[];
    };
    ProjectDetailDto: {
      id: string;
      projectGroupId: string;
      projectGroupName: components["schemas"]["LocalizedString"];
      genre: components["schemas"]["LocalizedString"];
      name: components["schemas"]["LocalizedString"];
      description: components["schemas"]["LocalizedString"];
      visibility: components["schemas"]["Visibility"];
      /** Format: date-time */
      releaseDate: string;
      crew: (components["schemas"]["ProjectAuthorDto"])[];
      cast: (components["schemas"]["ProjectAuthorDto"])[];
      artifacts: (components["schemas"]["ArtifactDetailDto"])[];
    };
    ProjectGroupDetailDto: {
      id: string;
      name: components["schemas"]["LocalizedString"];
      description: components["schemas"]["LocalizedString"];
      /** Format: date-time */
      deadline: string;
      isOpen: boolean;
    };
    ProjectGroupListDto: {
      id: string;
      name: components["schemas"]["LocalizedString"];
      description: components["schemas"]["LocalizedString"];
      /** Format: date-time */
      deadline: string;
      isOpen: boolean;
    };
    ProjectListDto: {
      id: string;
      projectGroupId: string;
      name: components["schemas"]["LocalizedString"];
      description: components["schemas"]["LocalizedString"];
      visibility: components["schemas"]["Visibility"];
      /** Format: date-time */
      releaseDate: string;
    };
    ShardDetailBaseDto: {
      Kind: string;
      /** Format: hrib */
      id: string;
      kind: components["schemas"]["ShardKind"];
      /** Format: hrib */
      artifactId: string;
    };
    /** @enum {string} */
    ShardKind: "Invalid" | "Video" | "Image" | "Subtitles";
    ShardListDto: {
      /** Format: hrib */
      id: string;
      kind: components["schemas"]["ShardKind"];
      variants: (string)[];
    };
    SubtitleStreamDto: {
      codec: string;
      /** Format: int64 */
      bitrate: number;
    };
    TimeSpan: {
      /** Format: int64 */
      ticks: number;
      /** Format: int32 */
      days: number;
      /** Format: int32 */
      hours: number;
      /** Format: int32 */
      milliseconds: number;
      /** Format: int32 */
      microseconds: number;
      /** Format: int32 */
      nanoseconds: number;
      /** Format: int32 */
      minutes: number;
      /** Format: int32 */
      seconds: number;
      /** Format: double */
      totalDays: number;
      /** Format: double */
      totalHours: number;
      /** Format: double */
      totalMilliseconds: number;
      /** Format: double */
      totalMicroseconds: number;
      /** Format: double */
      totalNanoseconds: number;
      /** Format: double */
      totalMinutes: number;
      /** Format: double */
      totalSeconds: number;
    };
    VideoShardDetailDto: ({
      Kind: "VideoShardDetailDto";
      variants: {
        [key: string]: components["schemas"]["MediaDto"] | undefined;
      };
    }) & Omit<components["schemas"]["ShardDetailBaseDto"], "Kind">;
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
