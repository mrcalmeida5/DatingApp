import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/_models/Member';
import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { MessageService } from 'src/app/_services/message.service';
import { Message } from 'src/app/_models/message';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {

  member: Member;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  messages: Message[] = [];
  @ViewChild('memberTabs', {static: true}) memberTabs: TabsetComponent;
  selectedTab: TabDirective;


  constructor(private route: ActivatedRoute, private messageService: MessageService) { }

  ngOnInit(): void {
    this.route.data.subscribe(data =>
      this.member = data.member);

    this.route.queryParams.subscribe(params => {
      params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    })

    this.galleryOptions = [{
      width: '500px',
      height: '500px',
      imagePercent: 100,
      thumbnailsColumns: 4,
      imageAnimation: NgxGalleryAnimation.Slide,
      preview: false
    }];

    this.galleryImages = this.getImages();
  }

  getImages(): NgxGalleryImage[] {
    const imagesUrls = [];
    for (var photo of this.member.photos) {
      imagesUrls.push({
        small: photo?.url,
        medium: photo?.url,
        big: photo?.url
      });
    }

      return imagesUrls;
  }

  loadMessageThread() {
    this.messageService.getMessageThread(this.member.id).subscribe(response =>
      this.messages = response);
  }

  selectTab(tabIndex: number) {
    this.memberTabs.tabs[tabIndex].active = true;
  }

  onTabSelected(tab: TabDirective) {
    this.selectedTab = tab;
    if (tab.heading === 'Messages' && this.messages.length === 0) {
      this.loadMessageThread();
    }
  }

}
