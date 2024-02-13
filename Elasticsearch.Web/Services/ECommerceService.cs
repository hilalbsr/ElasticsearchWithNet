using Elasticsearch.Web.Repositories;
using Elasticsearch.Web.ViewModels;

namespace Elasticsearch.Web.Services
{
	public class ECommerceService
	{

		private readonly ECommerceRepository _repository;

		public ECommerceService(ECommerceRepository repository)
		{
			_repository = repository;
		}

		public async Task<(List<ECommerceViewModel>, long totalCount, long pageLinkCount)> SearchAsync(ECommerceSearchViewModel searchModel, int page, int pageSize)
		{
			//list
			//totalCount
			// 1 2 3 4 5 6 7 8 9 10
			//52 % 10 = 2
			// 1 2 3 4 5 6

			var (eCommerceList, totalCount) = await _repository.SearchAsync(searchModel, page, pageSize);

			var pageLinkCountCalculate = totalCount % pageSize;
			long pageLinkCount = 0;

            pageLinkCount  = pageLinkCountCalculate == 0
                           ? totalCount / pageSize 
                           : (totalCount / pageSize) + 1;

			var eCommerceListViewModel = eCommerceList.Select(x => new ECommerceViewModel()
			{
				Category = String.Join(",", x.Category),
				CustomerFullName = x.CustomerFullName,
				CustomerFirstName = x.CustomerFirstName,
				CustomerLastName = x.CustomerLastName,
				OrderDate = x.OrderDate.ToShortDateString(),
				Gender = x.Gender.ToLower(),
				Id = x.Id,
				OrderId = x.OrderId,
				TaxfulTotalPrice = x.TaxfulTotalPrice
			}).ToList();


			return (eCommerceListViewModel, totalCount, pageLinkCount);
		}
	}
}
