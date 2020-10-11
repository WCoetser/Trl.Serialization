using TestObjects;

namespace Trl.Serialization.Tests.TestObjects
{
    public class PhoneInfo : ContactInfo
    {
        public string PhoneNumber { get; set; }

        public void Deconstructor(out string phoneNumber)
        {
            phoneNumber = PhoneNumber;
        }

        public void Deconstruct(out string phoneNumber, out string name)
        {
            phoneNumber = PhoneNumber;
            name = Name;
        }
    }
}
