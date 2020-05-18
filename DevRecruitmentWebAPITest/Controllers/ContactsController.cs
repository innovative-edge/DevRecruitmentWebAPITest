using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace DevRecruitmentWebAPITest.Controllers
{
    public class ContactsController : ApiController
    {
        public IHttpActionResult GetActive()
        {
            if (CheckIt() != Permissions.None) return Unauthorized();

            var dbContext = new ContactContext();
            var contacts = dbContext.Contacts.Where(c => c.IsActive).ToList();
            var viewModel = new List<ContactViewModel>();

            foreach(var contact in contacts)
            {
                var age = DateTime.Now.Year - contact.DateOfBirth.Value.Year;
                if (DateTime.Now.DayOfYear < contact.DateOfBirth.Value.DayOfYear)
                {
                    age = age - 1;
                }

                viewModel.Add(new ContactViewModel
                {
                    Id = contact.Id,
                    DateOfBirth = contact.DateOfBirth,
                    Age = age,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    FullName = contact.FirstName + " " + contact.LastName,
                    Email = contact.Email,
                    Phone = contact.Phone,
                    IsActive = contact.IsActive
                });
            }

            return Ok(viewModel);
        }

        public IHttpActionResult Get(int id)
        {
            if (CheckIt() != Permissions.None) return Unauthorized();

            var dbContext = new ContactContext();
            var contact = dbContext.Contacts.FirstOrDefault(c => c.Id == id);

            var age = DateTime.Now.Year - contact.DateOfBirth.Value.Year;
            if (DateTime.Now.DayOfYear < contact.DateOfBirth.Value.DayOfYear)
            {
                age = age - 1;
            }

            return Ok(new ContactViewModel
            {
                Id = contact.Id,
                DateOfBirth = contact.DateOfBirth,
                Age = age,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                FullName = contact.FirstName + " " + contact.LastName,
                Email = contact.Email,
                Phone = contact.Phone,
                IsActive = contact.IsActive
            });
        }

        public IHttpActionResult Post(ContactViewModel contact)
        {
            if ((CheckIt() == Permissions.None) || (CheckIt() == Permissions.ReadOnly)) return Unauthorized();

            if (string.IsNullOrEmpty(contact.FirstName) || string.IsNullOrEmpty(contact.LastName))
            {
                return BadRequest("First and last name must not be null or an empty string");
            }

            var dbContext = new ContactContext();
            dbContext.Contacts.Add(new ContactDto
            {
                DateOfBirth = contact.DateOfBirth,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                IsActive = contact.IsActive,
                Email = contact.Email,
                Phone = contact.Phone
            });

            return Ok(dbContext.SaveChanges());
        }

        public IHttpActionResult Put(int id, ContactViewModel contact)
        {
            if ((CheckIt() == Permissions.None) || (CheckIt() == Permissions.ReadOnly)) return Unauthorized();

            if (string.IsNullOrEmpty(contact.FirstName) || string.IsNullOrEmpty(contact.LastName))
            {
                return BadRequest("First and last name must not be null or an empty string");
            }

            var dbContext = new ContactContext();
            var contactDto = dbContext.Contacts.FirstOrDefault(c => c.Id == id);

            contactDto.DateOfBirth = contact.DateOfBirth;
            contactDto.FirstName = contact.FirstName;
            contactDto.IsActive = contact.IsActive;
            contactDto.LastName = contact.LastName;
            contactDto.Email = contact.Email;
            contactDto.Phone = contact.Phone;

            return Ok(dbContext.SaveChanges());
        }

        public IHttpActionResult Delete(int id)
        {
            if ((CheckIt() == Permissions.None) || (CheckIt() == Permissions.ReadOnly)) return Unauthorized();

            var dbContext = new ContactContext();
            var contactDto = dbContext.Contacts.FirstOrDefault(c => c.Id == id);

            dbContext.Contacts.Remove(contactDto);
            return Ok(dbContext.SaveChanges());
        }

        public Permissions CheckIt()
        {
            if (int.TryParse(HttpContext.Current.Session["UserId"].ToString(), out int userId))
            {
                var dbContext = new ContactContext();
                var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
                return (user == null || string.IsNullOrEmpty(user.Permissions)) ? Permissions.None : (Permissions)Enum.Parse(typeof(Permissions), user.Permissions);
            }

            return Permissions.None;
        }
    }


    public enum Permissions
    {
        None = 0,
        ReadOnly = 1,
        ReadWrite = 2,
        Admin = 3
    }

    public class ContactViewModel
    {
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserViewModel
    {
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public Permissions Permissions { get; set; }
    }

    [Table("Contact")]
    public class ContactDto
    {
        [Key]
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
    }

    [Table("User")]
    public class UserDto
    {
        [Key]
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public string Permissions { get; set; }
    }

    public class ContactContext : DbContext
    {
        public DbSet<ContactDto> Contacts { get; set; }
        public DbSet<UserDto> Users { get; set; }
    }

}
