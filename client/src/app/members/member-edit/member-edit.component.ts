import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/Member';
import { User } from 'src/app/_models/User';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {

  member: Member;
  user: User;

  @ViewChild('editForm') editForm: NgForm; 

  // To ask when we click close the tab (webpage) or the browser
  @HostListener('window:beforeunload', ['$event']) unsavedNotification($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }
  

  constructor(private accountService: AccountService,
    private memberService: MembersService, private toastrService: ToastrService) { 
      accountService.currentUser$.pipe(take(1)).subscribe(user =>
        this.user = user);
  }

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    this.memberService.getMember(this.user.id).subscribe(member =>
      this.member = member);
  }

  updateMember() {
    this.memberService.updateMember(this.member).subscribe(() => {
      this.toastrService.success('Profile updated successfuly');
      this.editForm.reset(this.member);
    })
  }


}

