using Elastic.Clients.Elasticsearch;
using Elasticsearch.API.DTOs;
using Elasticsearch.API.Models;
using System.Collections.Immutable;

namespace Elasticsearch.API.Repositories
{
    public class ProductRepository
    {
        private readonly ElasticsearchClient _client;
        private const string indexName = "products11";

        public ProductRepository(ElasticsearchClient client)
        {
            _client = client;
        }

        public async Task<Product?> SaveAsync(Product newProduct)
        {
            newProduct.Created = DateTime.Now;

            //newProduct Indexle. kaydet. yoksa oluştur.
            //indexName --tablo ismi
            //var response = await _client.IndexAsync(newProduct, x => x.Index(indexName));

            //77.Client tarafından ID’nin belirlenmesi ve BaseController sınıfının oluşturulması
            var response = await _client.IndexAsync(newProduct, x => x.Index(indexName).Id(Guid.NewGuid().ToString()));



            //fast fail
            if (!response.IsSuccess())
                return null;

            newProduct.Id = response.Id;

            return newProduct;
        }

        public async Task<ImmutableList<Product>> GetAllAsync()
        {
            //s-search
            //GET kibana_sample_data_ecommerce/ _search
            //{
            //    "query":{ "match_all":{ } }
            //}

            var result = await _client.SearchAsync<Product>(s => s.Index(indexName).Query(q => q.MatchAll()));

            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            //ToImmutableList --listede değişiklik yapmasın.
            return result.Documents.ToImmutableList();
        }

        public async Task<Product?> GetByIdAsync(string id)
        {

            var response = await _client.GetAsync<Product>(id, x => x.Index(indexName));

            if (!response.IsSuccess())
                return null;
            
            response.Source.Id = response.Id;
            return response.Source;
        }


        public async Task<bool> UpdateSynch(ProductUpdateDto updateProduct)
        {
            var response = await _client.UpdateAsync<Product, ProductUpdateDto>(indexName, updateProduct.Id, x=>x.Doc(updateProduct));

            return response.IsSuccess();
        }

        /// <summary>
        /// Hata yönetimi için bu method ele alınmıştır.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DeleteResponse> DeleteAsync(string id)
        {
            var response = await _client.DeleteAsync<Product>(id, x => x.Index(indexName));
            return response;
        }
    }
}
