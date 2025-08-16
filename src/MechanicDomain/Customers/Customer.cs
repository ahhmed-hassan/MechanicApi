using ErrorOr;
using MechanicDomain.Abstractions;
using MechanicDomain.Customers.Vehicles;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MechanicDomain.Customers;

    public sealed class Customer : AuditableEntity
    {
    //TODO: Wrap string in a Non NullableString type to ensure it is never null or empty
        public string? Name { get; private set; }
        public string? PhoneNumber { get; private set; }
        public string? Email { get; private set; }

        private readonly List<Vehicle> _vehicles = [];
        public IEnumerable<Vehicle> Vehicles { get; } = new List<Vehicle>();

        private Customer()
        { }

        private Customer(Guid id, string name, string phoneNumber, string email, List<Vehicle> vehicles)
            : base(id)
        {
            Name = name;
            PhoneNumber = phoneNumber;
            Email = email;
            Vehicles = vehicles;
        }

        private static ErrorOr<Guid> GetIdIfValid(Guid id, string name, string phoneNumber, string email)
        {
            if (string.IsNullOrWhiteSpace(name)) return CustomerErrors.NameRequired;


            if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
            {
                return CustomerErrors.InvalidPhoneNumber;
            }

            if (string.IsNullOrWhiteSpace(email)) return CustomerErrors.EmailRequired;


            try
            {
                _ = new MailAddress(email);
            }
            catch
            {
                return CustomerErrors.EmailInvalid;
            }
            return id;
        }
    public static ErrorOr<Customer> Create(Guid id, string name, string phoneNumber, string email, List<Vehicle> vehicles)
    {
        return GetIdIfValid(id, name, email, phoneNumber).Then(id =>
        new Customer(id, name, phoneNumber, email, vehicles)); 
    }

    public ErrorOr<Updated> Update(string name, string email, string phoneNumber)
    {
        return GetIdIfValid(Id, name, email, phoneNumber).
            ThenDo(Id => { Name = name; Email = email; PhoneNumber = phoneNumber; }). 
            Then(_ => Result.Updated) ; 
    }

    public ErrorOr<Updated> UpsertParts(List<Vehicle> incomingVehicle)
    {
        _vehicles.RemoveAll(existing => incomingVehicle.All(v => v.Id != existing.Id));

        foreach (var incoming in incomingVehicle)
        {
            var existing = _vehicles.FirstOrDefault(v => v.Id == incoming.Id);
            if (existing is null)
            {
                _vehicles.Add(incoming);
            }
            else
            {
                var updateVehicleResult = existing.Update(incoming.Make, incoming.Model, incoming.Year, incoming.LicensePlate);

                if (updateVehicleResult.IsError)
                {
                    return updateVehicleResult.Errors;
                }
            }
        }

        return Result.Updated;
    }

    }