import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ListingSearchItem {
  id: string;
  source: string;
  address: string;
  city: string;
  state: string;
  zip: string;
  price: number;
  bedrooms: number;
  relevanceScore: number;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ListingSearchRequest {
  minPrice?: number;
  maxPrice?: number;
  minBedrooms?: number;
  city?: string;
  keyword?: string;
  targetBudget?: number;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class ListingApiService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl =
  'https://potential-space-chainsaw-wvv6x7r4jprgc9697-5000.app.github.dev/api/listings/search';
  search(
    request: ListingSearchRequest
  ): Observable<PagedResult<ListingSearchItem>> {

    let params = new HttpParams()
      .set('page', request.page)
      .set('pageSize', request.pageSize);

    if (request.minPrice !== undefined) {
      params = params.set('minPrice', request.minPrice);
    }

    if (request.maxPrice !== undefined) {
      params = params.set('maxPrice', request.maxPrice);
    }

    if (request.minBedrooms !== undefined) {
      params = params.set('minBedrooms', request.minBedrooms);
    }

    if (request.city) {
      params = params.set('city', request.city);
    }

    if (request.keyword) {
      params = params.set('keyword', request.keyword);
    }

    if (request.targetBudget !== undefined) {
      params = params.set('targetBudget', request.targetBudget);
    }

    return this.http.get<PagedResult<ListingSearchItem>>(
      this.apiUrl,
      { params }
    );
  }
}
