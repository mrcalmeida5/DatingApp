import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getMessages(pageNumber, pageSize, container) {
    let params = new HttpParams();
    params = getPaginationHeaders(params, pageNumber, pageSize);
    params = params.append('Container', container);

    return getPaginatedResult<Message[]>(this.baseUrl + 'messages', params, this.http)
  }

  getMessageThread(userId: number) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + userId);
  }

  sendMessage(message: Message) {
    return this.http.post<Message>(this.baseUrl + 'messages',
      { recipientId: message.recipientId, content: message.content });
  }

  deleteMessage(messageId: number) {
    return this.http.delete(this.baseUrl + 'messages/' + messageId);
  }
}
