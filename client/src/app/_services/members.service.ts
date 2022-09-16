import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { LikesParams } from '../_models/likesParams';
import { Member } from '../_models/Member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/User';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';


@Injectable({
  providedIn: 'root'
})
export class MembersService {

  baseUrl = environment.apiUrl;
  members: Member[] = [];
  paginatedResult: PaginatedResult<Member[]> = new PaginatedResult<Member[]>();
  membersCached = new Map();
  userParams: UserParams;
  user: User;

  likesParams: LikesParams = new LikesParams();

  constructor(private http: HttpClient, private accountService: AccountService) {
    accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(this.user);
    });
   }


  getMembers(userParams: UserParams) {

    var response = this.membersCached.get(Object.values(userParams).join('-'));
    if (response)
      return of(response);

    let params = new HttpParams();
    params = getPaginationHeaders(params, userParams.pageNumber, userParams.pageSize);
    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy)

    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params, this.http)
      .pipe(map(response => {
        this.membersCached.set(Object.values(userParams).join('-'), response);
        return response;
      }));
  }

  getMember(id: number) {
    
    const member = [...this.membersCached.values()]
      .reduce((prev, current) => prev.concat(current.result), [])
      .find((x : Member) => x.id === id);
    
    if (member)
      return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + id);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      }
      ));
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }



  addLike(likedUserId: number) {
    return this.http.post(this.baseUrl + 'likes/' + likedUserId, {});
  }

  getLikes(likesParams: LikesParams) {

    let params = new HttpParams();
    params = getPaginationHeaders(params, likesParams.pageNumber, likesParams.pageSize);
    params = params.append('predicate', likesParams.predicate);

    return getPaginatedResult<Partial<Member[]>>(this.baseUrl + 'likes', params, this.http);
  }

  getLikesParams() {
    return this.likesParams;
  }

  setLikeParams(newParams: LikesParams) {
    this.likesParams = newParams;
  }

}
