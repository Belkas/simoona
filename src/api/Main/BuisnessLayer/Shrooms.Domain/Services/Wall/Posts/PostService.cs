﻿using System;
using System.Data.Entity;
using System.Linq;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Users;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;

namespace Shrooms.Domain.Services.Wall.Posts
{
    public class PostService : IPostService
    {
        private static object postDeleteLock = new object();

        private readonly IPermissionService _permissionService;
        private readonly IPostNotificationService _postNotificationService;
        private readonly ICommentService _commentService;

        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<Post> _postsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<WallModerator> _moderatorsDbSet;
        private readonly IDbSet<EntityModels.Models.Multiwall.Wall> _wallsDbSet;

        public PostService(
            IUnitOfWork2 uow,
            IPermissionService permissionService,
            IPostNotificationService postNotificationService,
            ICommentService commentService)
        {
            _uow = uow;
            _permissionService = permissionService;
            _postNotificationService = postNotificationService;
            _commentService = commentService;

            _postsDbSet = uow.GetDbSet<Post>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _moderatorsDbSet = uow.GetDbSet<WallModerator>();
            _wallsDbSet = uow.GetDbSet<EntityModels.Models.Multiwall.Wall>();
        }

        public NewlyCreatedPostDTO CreateNewPost(NewPostDTO newPostDto)
        {
            var wall = _wallsDbSet
                .Where(x => x.Id == newPostDto.WallId && x.OrganizationId == newPostDto.OrganizationId).
                FirstOrDefault();

            if (wall == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Wall not found");
            }

            var post = new Post
            {
                AuthorId = newPostDto.UserId,
                Created = DateTime.UtcNow,
                LastEdit = DateTime.UtcNow,
                CreatedBy = newPostDto.UserId,
                MessageBody = newPostDto.MessageBody,
                PictureId = newPostDto.PictureId,
                SharedEventId = newPostDto.SharedEventId,
                LastActivity = DateTime.UtcNow,
                WallId = newPostDto.WallId,
                Likes = new LikesCollection()
            };
            _postsDbSet.Add(post);
            _uow.SaveChanges(newPostDto.UserId);

            var postCreator = _usersDbSet.Single(user => user.Id == newPostDto.UserId);
            var postCreatorDto = MapUserToDto(postCreator);
            var newlyCreatedPostDto = MapNewlyCreatedPostToDto(post, postCreatorDto, wall.Type);

            _postNotificationService.NotifyAboutNewPost(post, postCreator);
            return newlyCreatedPostDto;
        }

        public void ToggleLike(int postId, UserAndOrganizationDTO userOrg)
        {
            var post = _postsDbSet
                .Include(x => x.Wall)
                .FirstOrDefault(x =>
                    x.Id == postId &&
                    x.Wall.OrganizationId == userOrg.OrganizationId);

            if (post == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post does not exist");
            }

            var like = post.Likes.FirstOrDefault(x => x.UserId == userOrg.UserId);
            if (like == null)
            {
                post.Likes.Add(new Like(userOrg.UserId));
            }
            else
            {
                post.Likes.Remove(like);
            }

            _uow.SaveChanges(userOrg.UserId);
        }

        public void EditPost(EditPostDTO editPostDto)
        {
            var post = _postsDbSet
                .Include(x => x.Wall)
                .FirstOrDefault(x =>
                    x.Id == editPostDto.Id &&
                    x.Wall.OrganizationId == editPostDto.OrganizationId);

            if (post == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post not found");
            }

            var isWallModerator = _moderatorsDbSet
                .Any(x => x.UserId == editPostDto.UserId && x.WallId == post.WallId) || post.CreatedBy == editPostDto.UserId;

            var isAdministrator = _permissionService.UserHasPermission(editPostDto, AdministrationPermissions.Post);

            if (!isAdministrator && !isWallModerator)
            {
                throw new UnauthorizedException();
            }

            post.MessageBody = editPostDto.MessageBody;
            post.PictureId = editPostDto.PictureId;
            post.LastEdit = DateTime.UtcNow;

            _uow.SaveChanges(editPostDto.UserId);
        }

        public void DeleteWallPost(int postId, UserAndOrganizationDTO userOrg)
        {
            lock (postDeleteLock)
            {
                var post = _postsDbSet
                    .Include(x => x.Wall)
                    .FirstOrDefault(s =>
                        s.Id == postId &&
                        s.Wall.OrganizationId == userOrg.OrganizationId);

                if (post == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post not found");
                }

                var isWallModerator = _moderatorsDbSet
                    .Any(x => x.UserId == userOrg.UserId && x.WallId == post.WallId);

                var isAdministrator = _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Post);
                if (!isAdministrator && !isWallModerator)
                {
                    throw new UnauthorizedException();
                }

                _commentService.DeleteCommentsByPost(post.Id, userOrg);
                _postsDbSet.Remove(post);

                _uow.SaveChanges(userOrg.UserId);
            }
        }

        public void HideWallPost(int postId, UserAndOrganizationDTO userOrg)
        {
            lock (postDeleteLock)
            {
                var post = _postsDbSet
                    .Include(x => x.Wall)
                    .FirstOrDefault(s =>
                        s.Id == postId &&
                        s.Wall.OrganizationId == userOrg.OrganizationId);

                if (post == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post not found");
                }

                var isWallModerator = _moderatorsDbSet
                    .Any(x => x.UserId == userOrg.UserId && x.WallId == post.WallId) || post.AuthorId == userOrg.UserId;

                var isAdministrator = _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Post);
                if (!isAdministrator && !isWallModerator)
                {
                    throw new UnauthorizedException();
                }

                post.IsHidden = true;
                post.LastEdit = DateTime.UtcNow;

                _uow.SaveChanges(userOrg.UserId);
            }
        }

        private UserDto MapUserToDto(ApplicationUser user)
        {
            var userDto = new UserDto
            {
                UserId = user.Id,
                FullName = user.FirstName + ' ' + user.LastName,
                PictureId = user.PictureId,
            };
            return userDto;
        }

        private NewlyCreatedPostDTO MapNewlyCreatedPostToDto(Post post, UserDto user, WallType wallType)
        {
            var newlyCreatedPostDto = new NewlyCreatedPostDTO
            {
                Id = post.Id,
                MessageBody = post.MessageBody,
                PictureId = post.PictureId,
                Created = post.Created,
                CreatedBy = post.CreatedBy,
                User = user,
                WallType = wallType
            };
            return newlyCreatedPostDto;
        }
    }
}