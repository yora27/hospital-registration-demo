using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Hosreg1
{
    public class Review
    {
        public int Id { get; set; }
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
    }

    public class Patient : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private string _login;
        private string _password;
        private List<string> _comments;
        private Doctor _assignedDoctor;
        private DateTime? _appointmentTime;
        public string Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Address { get; set; }
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Login
        {
            get { return _login; }
            set
            {
                if (_login != value)
                {
                    _login = value;
                    OnPropertyChanged(nameof(Login));
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public List<string> Comments
        {
            get { return _comments; }
            set
            {
                if (_comments != value)
                {
                    _comments = value;
                    OnPropertyChanged(nameof(Comments));
                }
            }
        }

        public Doctor AssignedDoctor
        {
            get { return _assignedDoctor; }
            set
            {
                if (_assignedDoctor != value)
                {
                    _assignedDoctor = value;
                    OnPropertyChanged(nameof(AssignedDoctor));
                }
            }
        }

        public DateTime? AppointmentTime
        {
            get { return _appointmentTime; }
            set
            {
                if (_appointmentTime != value)
                {
                    _appointmentTime = value;
                    OnPropertyChanged(nameof(AppointmentTime));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddComment(string comment)
        {
            Comments.Add(comment);
            OnPropertyChanged(nameof(Comments));
        }
    }

    public class Doctor : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private string _specialization;
        private List<TimeSlot> _availableTimeSlots;

        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Specialization
        {
            get { return _specialization; }
            set
            {
                if (_specialization != value)
                {
                    _specialization = value;
                    OnPropertyChanged(nameof(Specialization));
                }
            }
        }

        public List<TimeSlot> AvailableTimeSlots
        {
            get { return _availableTimeSlots; }
            set
            {
                if (_availableTimeSlots != value)
                {
                    _availableTimeSlots = value;
                    OnPropertyChanged(nameof(AvailableTimeSlots));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class TimeSlot
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class Appointment
    {
        public int Id { get; set; }
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public static class Database
    {
        public static List<Patient> Patients { get; set; } = new List<Patient>();
        public static List<Doctor> Doctors { get; set; } = new List<Doctor>();
        public static List<Appointment> Appointments { get; set; } = new List<Appointment>();
        public static List<Review> Reviews { get; set; } = new List<Review>();


        internal class Data
        {
        }
    }
}

