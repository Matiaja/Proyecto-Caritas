import { Stock } from './stock.model';

export interface Product {
  id: number;
  name: string;
  categoryId: number;
  stocks: Stock[];
}
