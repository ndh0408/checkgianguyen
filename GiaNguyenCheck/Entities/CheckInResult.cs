using System;

namespace GiaNguyenCheck.Entities
{
    public class CheckInResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public Guest? Guest { get; set; }
        public DateTime? CheckInTime { get; set; }
    }
} 