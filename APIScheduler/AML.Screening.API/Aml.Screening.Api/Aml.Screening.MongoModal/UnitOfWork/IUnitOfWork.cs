using System;
using System.Collections.Generic;
using System.Text;

namespace Aml.Screening.MongoModal.UnitOfWork
{
    public interface IUnitOfWork
    {
        int Save();
    }
}
