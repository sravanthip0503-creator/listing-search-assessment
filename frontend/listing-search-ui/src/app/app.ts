import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import {
  ListingApiService,
  ListingSearchItem
} from './services/listing-api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly listingApi = inject(ListingApiService);

  minPrice: number | null = null;
  maxPrice: number | null = null;
  minBedrooms: number | null = null;
  city = '';
  keyword = '';
  targetBudget: number | null = null;
  pageSize = 5;

  currentPage = 1;
  totalPages = 0;
  totalCount = 0;

  listings: ListingSearchItem[] = [];

  loading = false;
  errorMessage = '';

  search(): void {
    this.currentPage = 1;
    this.loadListings();
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadListings();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadListings();
    }
  }

  private loadListings(): void {
    this.loading = true;
    this.errorMessage = '';

    this.listingApi.search({
      minPrice: this.minPrice ?? undefined,
      maxPrice: this.maxPrice ?? undefined,
      minBedrooms: this.minBedrooms ?? undefined,
      city: this.city.trim() || undefined,
      keyword: this.keyword.trim() || undefined,
      targetBudget: this.targetBudget ?? undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
  console.log('ANGULAR API RESPONSE:', response);

  this.listings = response.items;
  this.currentPage = response.page;
  this.totalPages = response.totalPages;
  this.totalCount = response.totalCount;
  this.loading = false;
},
      error: (error) => {
        this.listings = [];
        this.loading = false;

        this.errorMessage =
          error?.error?.detail ??
          'Unable to load listings. Please try again.';
      }
    });
  }
}