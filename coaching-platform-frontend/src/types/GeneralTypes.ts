export interface PaginatedResult<T> {
  page: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
  searchTerm: string | null;
  sortingCriteria: string | null;
  sortingOrder: string | null;
  data: T[];
}

export interface PaginatedApiResponse<T> {
  version: string | null;
  statusCode: number;
  message: string | null;
  isError: boolean | null;
  responseException: unknown | null;
  result: PaginatedResult<T>;
}

export interface ApiResponse<T> {
  version: string | null;
  statusCode: number;
  message: string | null;
  isError: boolean | null;
  responseException: unknown | null;
  result: T;
}

export interface ValidationErrorResponse {
  type: string;
  title: string;
  status: number;
  errors: Record<string, string[]>;
  traceId: string;
}
