

using KKProject.Data;
using KKProject.Migrations;
using KKProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Reviews.WebApi.Models.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Reviews.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly ResponseDto _response;
        protected StudentDbContext _context; 
        private static readonly HttpClient client = new HttpClient();
        private readonly string _clientId = "1e8765ee995943618fa8d5a1ca7e427d";
        private readonly string _clientSecret = "0f0af20c5dfa7a0f7b89b541c112af52";
        private readonly string _userName = "Karankhanna.kk@outlook.com";
        private readonly string _password = "Khanna72#";
        private readonly long _lockId = 11570260;
        public StudentController(StudentDbContext studentDbContext)
        {
            _context = studentDbContext;
            _response = new();
        }
        // GET: api/Student
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var students = _context.Students.ToList();
                _response.Result = students;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return Ok(_response);
        }
        // GET: api/Review/5
        [HttpGet("{id}", Name = "GetSudent")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var student = _context.Students.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
                _response.Result = student;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return Ok(_response);
        }
        // POST: api/Review
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StudentDto studentDto)
        {
            try
            {

                var accesstoken = await getdata();
                bool isNewUser;
                long ekey;
                Student student = new()
                {
                    Id = Guid.NewGuid(),
                    Name = studentDto.Name,
                    Email = studentDto.Email,
                    StartTime = studentDto.StartTime,
                    IsLocker = studentDto.IsLocker,
                    TotalAmount = studentDto.TotalAmount,
                    DurationInHours = studentDto.DurationInHours,
                    CreatedAt = DateTime.Now
                };
                var studentExist = _context.Students.AsQueryable().FirstOrDefault(x => x.Email == student.Email);
                var NewEndtime = student.StartTime.AddHours(student.DurationInHours);
                
                if (studentExist == null)
                {
                    ekey = CreateUser(accesstoken, studentDto.Email, studentDto.StartTime, NewEndtime, true);
                }
                else
                {
                    var ExistingEndTime = studentExist.StartTime.AddHours(studentExist.DurationInHours);
                    bool overlap = studentExist.StartTime < NewEndtime && student.StartTime < ExistingEndTime;
                    if (!overlap)
                    {
                        ekey = CreateUser(accesstoken, studentDto.Email, studentDto.StartTime, NewEndtime, false);
                    }
                    else
                    {

                        _response.IsSuccess = false;
                        _response.Message = "your time with your existing time";
                        return Ok(_response);
                    }
                    
                }

                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                _response.Result = ekey;
                
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return Ok(_response);
        }
        private async Task<string> getdata()
        {
            
            string password;
            using (MD5 md5Hash = MD5.Create())
            {

                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(_password));
                StringBuilder sBuilder = new StringBuilder();  
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                password=sBuilder.ToString();
            }
            var url = $"https://euapi.ttlock.com/oauth2/token?clientId={_clientId}&clientSecret={_clientSecret}&username={_userName}&password={password}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/x-www-form-urlencoded";

            request.Method = "POST";

            var response = (HttpWebResponse)request.GetResponse();


            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            AuthResponse ar =JsonConvert.DeserializeObject<AuthResponse>(responseString);

            return ar.access_token;
        }

        private long CreateUser(string aceestoken,string email,DateTime start,DateTime end, bool isNewUser)
        {
            DateTimeOffset now = new DateTimeOffset(DateTime.Now);
            DateTimeOffset startTime = new DateTimeOffset(start);
            DateTimeOffset endTime = new DateTimeOffset(end);



            var url = $"https://euapi.ttlock.com/v3/key/send?clientId={_clientId}&accessToken={aceestoken}&lockId={_lockId}&receiverUsername={email}&keyName={_userName}" +
                $"&startDate={startTime.ToUnixTimeMilliseconds()}&endDate={endTime.ToUnixTimeMilliseconds()}&createUser={(isNewUser ? 1 : 2)}&date={now.ToUnixTimeMilliseconds()}";
            

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            var r = DateTime.Now.Millisecond;

            request.ContentType = "application/x-www-form-urlencoded";

            request.Method = "POST";

            var response = (HttpWebResponse)request.GetResponse();


            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            GetKey getKey = JsonConvert.DeserializeObject<GetKey>(responseString);

            return getKey.keyId;
        }
        //// PUT: api/Review/5
        //[HttpPut]
        //[Authorize]
        //public async Task<IActionResult> Put([FromBody] ReviewDto reviewDto)
        //{
        //    try
        //    {
        //        Review obj = _mapper.Map<Review>(reviewDto);
        //        await _ReviewService.UpdateReview(obj);
        //        _response.Result = obj;
        //    }
        //    catch (Exception ex)
        //    {
        //        _response.IsSuccess = false;
        //        _response.Message = ex.Message;
        //    }

        //    return Ok(_response);
        //}
        //// DELETE: api/Review/5
        //[HttpDelete("{id}")]
        //[Authorize]
        //public async Task<IActionResult> Delete(Guid id)
        //{
        //    try
        //    {
        //        if (id != null && id != Guid.Empty)
        //        {
        //            await _ReviewService.DeleteReview(id);
        //        }
        //        _response.Result = id;
        //    }
        //    catch (Exception ex)
        //    {
        //        _response.IsSuccess = false;
        //        _response.Message = ex.Message;
        //    }

        //    return Ok(_response);
        //}
    }
}
