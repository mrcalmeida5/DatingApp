export interface Message {
    id: number;
    senderId: number;
    senderKnownAs: string;
    senderPhotoUrl?: string;
    recipientId: number;
    recipientKnownAs: string;
    recipientPhotoUrl?: string;
    content: string;
    dateRead?: Date;
    messageSent: Date;
}