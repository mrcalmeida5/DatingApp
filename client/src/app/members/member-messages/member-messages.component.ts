import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {

  @Input() userId: number;
  @Input() messages: Message[];
  newMessage: Message;
  @ViewChild('messageForm') messageForm: NgForm;

  constructor(private messageService: MessageService) { 
  }

  ngOnInit(): void {
    this.newMessage = {} as Message;
    this.newMessage.recipientId = this.userId;

  }

  sendMessage() {
    if (this.newMessage != null && this.newMessage.recipientId !== undefined && this.newMessage.content !== undefined) {
      this.messageService.sendMessage(this.newMessage).subscribe(message => {
        this.messages.push(message);
        this.messageForm.reset();
      });
    }
  }
}
