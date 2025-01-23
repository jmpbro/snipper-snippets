using System;

public class User
{
    public int Id {get; set;} // Primary Key         
    public string Username {get; set;}
    public string Email {get; set;}
    public string PasswordHash {get; set;} // Hashed password

    public DateTime CreatedAt {get; set;} = DateTime.Now;
}