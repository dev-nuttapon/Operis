export interface PaginatedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface PaginationInput {
  page?: number;
  pageSize?: number;
}
