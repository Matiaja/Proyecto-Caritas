export interface Stock {
    id: number;
    centerId: number;
    productId: number;
    entryDate: Date;
    expirationDate: Date;
    description: string;
    quantity: number;
    weight: number;
    status: string;
}