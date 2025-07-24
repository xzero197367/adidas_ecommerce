using Adidas.Application.Contracts.RepositoriesContracts.People;
using Adidas.Context;
using Models.People;

namespace Adidas.Infra.People;

public class AddressRepository: GenericRepository<Address>, IAddressRepository
{
    public AddressRepository(AdidasDbContext context) : base(context) { }
}