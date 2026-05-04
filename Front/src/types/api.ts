export interface Pagination {
  page: number
  pageSize: number
  totalRecords: number
}

export interface ApiError {
  code: string
  details?: unknown
}

export interface ApiResponse<T> {
  data: T
  pagination?: Pagination
  success: boolean
  message?: string
  error?: ApiError
}
