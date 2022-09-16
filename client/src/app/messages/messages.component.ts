import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {

  messages: Message[] = [];
  pagination: Pagination;
  pageNumber = 1;
  pageSize = 5;
  container = 'Unread';
  loading = false;

  constructor(private messagesService: MessageService) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages()
  {
    this.loading = true;

    this.messagesService.getMessages(this.pageNumber, this.pageSize, this.container).subscribe(response => {
      this.messages = response.result;
      this.pagination = response.pagination;

      this.loading = false;
    })
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }

  deleteMessage(messageId: number) {
    this.messagesService.deleteMessage(messageId).subscribe(() =>
      this.messages.splice(this.messages.findIndex(m => m.id === messageId), 1));
  }

}
