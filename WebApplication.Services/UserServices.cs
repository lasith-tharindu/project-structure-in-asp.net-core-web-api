﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication.Common.Common;
using WebApplication.Common.Helpers;
using WebApplication.Common.Models;
using WebApplication.Common.Utilities;
using WebApplication.Data.Interfaces;
using WebApplication.Services.Interfaces;

namespace WebApplication.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepository userRepository;

        public UserServices(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public ActionResponse<IList<UserModal>> GetAllUser()
        {
            var result = new ActionResponse<IList<UserModal>>();

            try
            {
                result.Data = userRepository.GetAllUser();
                result.Success = true;
            }
            catch (Exception ex)
            {

                result.Data = new List<UserModal>();
                result.Message = ex.Message;
            }

            return result;
        }

        public ActionResponse<UserModal> SaveUser(UserModal userModal)
        {
            var result = new ActionResponse<UserModal>();

            try
            {
                if (userModal.Password != null)
                {
                    userModal.Password = AuthUtlis.GetHashed(userModal.Password);
                }

                var user = userRepository.SaveUser(userModal);
                if (user.UserId > 0)
                {
                    user.Password = "";
                    result.Data = user;
                    result.Success = true;
                    result.Message = "User added successfully.";
                }

            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }


        public ActionResponse<UserModal> EditUser(UserModal userModal)
        {
            var result = new ActionResponse<UserModal>();

            try
            {
                if (userModal.Password != null)
                {
                    userModal.Password = AuthUtlis.GetHashed(userModal.Password);
                }

                var user = userRepository.EditUser(userModal);

                result.Data = user;
                result.Success = true;
                result.Message = "User updated successfully.";
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        public ActionResponse<UserModal> GetUserByUserId(int userId)
        {
            var result = new ActionResponse<UserModal>();

            try
            {
                if (userId != 0)
                {
                    var user = userRepository.GetUserByUserId(userId);

                    result.Data = user;
                    result.Success = true;

                }
                else
                {
                    result.Success = false;
                    result.Message = "Please enter userId.";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        public ActionResponse<UserModal> DeleteUserByUserId(int userId)
        {
            var result = new ActionResponse<UserModal>();

            try
            {
                if (userId != 0)
                {
                    var user = userRepository.DeleteUserByUserId(userId);

                    if (user)
                    {
                        result.Message = "User deleted successfully.";
                        result.Success = true;
                    }
                    else
                    {
                        result.Message = "User not deleted.";
                        result.Success = false;
                    }

                }
                else
                {
                    result.Success = false;
                    result.Message = "Please enter userId.";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        public ActionResponse<UserModal> GetCheckUser(string token)
        {
            var result = new ActionResponse<UserModal>();
            try
            {
                var tokenVerify = JwtService.JwtVerify(token);

                var user = userRepository.GetByUsername(tokenVerify.Issuer);
                if (user != null)
                {

                    var userModal = new UserModal();
                    if (user != null)
                    {
                        userModal.UserId = user.UserId;
                        userModal.FirstName = user.FirstName;
                        userModal.LastName = user.LastName;

                    }

                    result.Data = userModal;
                    result.Success = true;
                    result.Message = "You have successfully obtained user details.";
                }
                else
                {
                    result.Message = "User not found.";
                }
            }

            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        public ActionResponse<AuthenticateResponseModal> SignIn(string username, string password)
        {
            var result = new ActionResponse<AuthenticateResponseModal>();
            try
            {
                if (username != string.Empty && password != string.Empty)
                {

                    var user = userRepository.GetByUsername(username);
                    if (user == null)
                    {
                        result.Message = "User not found.";
                    }
                    else
                    {

                        if (AuthUtlis.VerifyPassword(password, user.Password))
                        {

                            var jwt = JwtService.Generate(user.UserName);
                            var refreshToken = JwtService.GenerateRefreshToken(user.UserName);
                            if (jwt != null)
                            {

                              //  user.Token = jwt;
                                user.Password = string.Empty;

                                var userModal = new AuthenticateResponseModal();
                                if (user != null)
                                {
                                    userModal.UserId = user.UserId;
                                    userModal.FirstName = user.FirstName;
                                    userModal.LastName = user.LastName;
                                    userModal.Token = jwt;


                                }

                                result.Data = userModal;
                                result.Data.RefreshToken = refreshToken;
                                result.Success = true;
                                result.Message = "Successfully logged in.";
                            }
                        }
                        else
                        {
                            result.Message = "Incorrect password.";
                        }

                    }
                }
                else
                {
                    result.Message = "All information is needed.";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
