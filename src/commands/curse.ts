export type CursePagination = {
  index: number;
  pageSize: number;
  resultCount: number;
  totalCount: number;
};

export type CurseFileModule = {
  name: string;
  fingerprint: number;
};

export type CurseFile = {
  id: number;
  gameId: number;
  modId: number;
  isAvailable: boolean;
  displayName: string;
  fileName: string;
  //   releaseType: CF2FileReleaseType;
  //   fileStatus: CF2FileStatus;
  //   hashes: CF2FileHash[];
  fileDate: string;
  fileLength: number;
  downloadCount: number;
  downloadUrl: string;
  gameVersions: string[];
  //   sortableGameVersions: CF2SortableGameVersion[];
  //   dependencies: CF2FileDependency[];
  exposeAsAlternative?: boolean;
  parentProjectFileId?: number;
  alternateFileId?: number;
  isServerPack?: number;
  serverPackFileId?: number;
  fileFingerprint: number;
  modules: CurseFileModule[];
};

export type CurseFileIndex = {
  gameVersion: string;
  fileId: number;
  filename: string;
  releaseType: 1 | 2 | 3;
  gameVersionTypeId?: number;
  // modLoader?: CF2ModLoaderType;
};

export type CurseAddon = {
  id: number;
  gameId: number;
  name: string;
  slug: string;
  //   links: CF2AddonLinks;
  summary: string;
  //   status: CF2ModStatus;
  downloadCount: number;
  isFeatured: boolean;
  primaryCategoryId: number;
  //   categories: CF2Category[];
  //   authors: CF2Author[];
  //   logo: CF2Asset;
  //   screenshots: CF2Asset[];
  mainFileId: number;
  latestFiles: CurseFile[];
  latestFilesIndexes: CurseFileIndex[];
  dateCreated: string;
  dateModified: string;
  dateReleased: string;
  allowModDistribution?: boolean;
  gamePopularityRank: number;
  isAvailable: boolean;
  thumbsUpCount?: number;
};

export type CurseSearchModsResponse = {
  data: CurseAddon[];
  pagination: CursePagination;
};
