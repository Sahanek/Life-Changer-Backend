using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace Tests
{
    public class FakeUserManagerBuilder
    {
        private Mock<FakeUserManager> _mock = new Mock<FakeUserManager>();

        public FakeUserManagerBuilder With(Action<Mock<FakeUserManager>> mock)
        {
            mock(_mock);
            return this;
        }

        public Mock<FakeUserManager> Build()
        {
            return _mock;
        }
    }

}
