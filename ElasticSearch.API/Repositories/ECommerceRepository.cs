using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.TermVectors;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elasticsearch.API.Models.ECommerceModel;
using Microsoft.Extensions.Hosting;
using System.Collections.Immutable;
using System.Numerics;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Elasticsearch.API.Repositories
{
    public class ECommerceRepository
    {
        private readonly ElasticsearchClient _client;
        public ECommerceRepository(ElasticsearchClient client)
        {
            _client = client;
        }

        private const string indexName = "kibana_sample_data_ecommerce";


        public async Task<ImmutableList<ECommerce>> TermQuery(string customerFirstName)
        {

            //GET kibana_sample_data_ecommerce/ _search
            //{
            //    "query": {
            //        "term": {
            //            "customer_first_name.keyword": {
            //                "value": "Youssef"
            //            }
            //        }
            //    }
            //}


            //1. way
            //var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
            //.Query(q => q.Term(t => t.Field("customer_first_name.keyword")
            //.Value(customerFirstName))));

            //2. way
            //var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
            //.Query(q => q.Term(t => t.CustomerFirstName.Suffix("keyword"), customerFirstName)));

            //3. way

            var termQuery = new TermQuery("customer_first_name.keyword")
            {
                Value = customerFirstName,
                CaseInsensitive = true
            };

            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName).Query(termQuery));

            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();

        }


        //GET kibana_sample_data_ecommerce/_search
        //{
        //  "query": {
        //    "terms": {
        //      "customer_first_name.keyword": [
        //        "Youssef",
        //        "Sonya"
        //      ]
        //    }
        // }
        //}
        public async Task<ImmutableList<ECommerce>> TermsQuery(List<string> customerFirstNameList)
        {
            List<FieldValue> terms = new List<FieldValue>();
            customerFirstNameList.ForEach(x =>
            {
                terms.Add(x);
            });

            //1.way
            //var termsQuery = new TermsQuery()
            //{
            //    Field = "customer_first_name.keyword",
            //    Terms = new TermsQueryField(terms.AsReadOnly())
            //};

            //var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName).Query(termsQuery));

            // 2. way
            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
            .Size(100)
            .Query(q => q
            .Terms(t => t
            .Field(f => f.CustomerFirstName
            .Suffix("keyword"))
            .Terms(new TermsQueryField(terms.AsReadOnly())))));


            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();
        }

        //GET kibana_sample_data_ecommerce/_search
        //{
        //  "query": {
        //    "prefix": {
        //      "customer_first_name.keyword": {
        //        "value": "Br"
        //      }
        //    }
        //  }
        //}
        public async Task<ImmutableList<ECommerce>> PrefixQueryAsync(string CustomerFullName)
        {
            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
                .Query(q => q
                    .Prefix(p => p
                        .Field(f => f.CustomerFullName
                            .Suffix("keyword"))
                                .Value(CustomerFullName))));

            return result.Documents.ToImmutableList();

        }

        //GET kibana_sample_data_ecommerce/_search
        //{
        //  "query": {
        //    "range": {
        //      "taxless_total_price": {
        //        "gt": 33.98,
        //        "lte": 40
        //      }
        //    }
        //  }
        //}
        public async Task<ImmutableList<ECommerce>> RangeQueryAsync(double FromPrice, double ToPrice)
        {
            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName).Size(20)
                .Query(q => q
                    .Range(r => r
                        .NumberRange(nr => nr
                            .Field(f => f.TaxfulTotalPrice)
                                .Gte(FromPrice).Lte(ToPrice)))));


            return result.Documents.ToImmutableList();
        }

        //GET products/_search
        //{
        //  "query":{"match_all":{}}
        //}
        //100 datanın getirilmesi
        public async Task<ImmutableList<ECommerce>> MatchAllQueryAsync()
        {

            //var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
            //    .Size(100)
            //    .Query(q => q.MatchAll()));


            var result = await _client.SearchAsync<ECommerce>(s =>
                s.Index(indexName).Size(1000).Query(q => q.Match(m => m.Field(f => f.CustomerFullName).Query("shaw"))));



            foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
            return result.Documents.ToImmutableList();

        }

        public async Task<ImmutableList<ECommerce>> PaginationQueryAsync(int page, int pageSize)
        {

            // page=1, pageSize=10 =>  1-10
            // page=2 , pageSize=10=> 11-20
            // page=3, pageSize=10 => 21-30


            var pageFrom = (page - 1) * pageSize;


            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
                .Size(pageSize).From(pageFrom)
                .Query(q => q.MatchAll()));


            foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
            return result.Documents.ToImmutableList();

        }

        //Sonunda ya da ortasında karaketerli olan aramalar.

        //* çoklu karakter
        //? tekli karakter

        //lam     --ile başlayan sonrasını bilmediğimiz.
        //lam*rt  -- lam ile başlayan sonu rt olan
        //lambe? t --tek bir karakteri bilmiyorsak

        //GET kibana_sample_data_ecommerce/_search
        //{
        //    "query": {
        //        "wildcard": {
        //            "customer_first_name.keyword": {
        //                "value": "*bi?it*"
        //            }
        //        }
        //    }
        //}

        public async Task<ImmutableList<ECommerce>> WildCardQueryAsync(string customerFullName)
        {



            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)

                .Query(q => q.Wildcard(w =>
                    w.Field(f => f.CustomerFullName
                            .Suffix("keyword"))
                                .Wildcard(customerFullName))));


            foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
            return result.Documents.ToImmutableList();

        }

        //Kullanıcı arama yaptığında harf hatası yaptıysa bu hatayı telafi edererk arama yapmakta.
        //Stephane yi tephanie yazarsa başındaki S harfi olanı da getiriyor.

        //Cake aranacak kelime:
        //take bake
        //lake make    nake

        //GET kibana_sample_data_ecommerce/_search
        //{
        //    "query": {
        //        "fuzzy": {
        //            "customer_first_name.keyword": {
        //                "value": "ephanie","fuzziness":2
        //            }
        //        }
        //    } 
        //}
        //GET kibana_sample_data_ecommerce/_search
        //{
        //  "size": 100, 
        //  "query": {
        //    "term": {
        //      "customer_gender": {
        //        "value": "MALE"
        //      }
        //    }
        //  },
        //  "sort": [
        //    {
        //      "customer_first_name.keyword": {
        //        "order": "desc"
        //      }
        //}
        //  ]
        //}

        public async Task<ImmutableList<ECommerce>> FuzzyQueryAsync(string customerName)
        {
            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
                .Query(q => q.Fuzzy(fu =>
                    fu.Field(f => f.CustomerFirstName.Suffix("keyword")).Value(customerName)
                        .Fuzziness(new Fuzziness(2))))
                            .Sort(sort => sort
                                .Field(f => f.TaxfulTotalPrice, new FieldSort() { Order = SortOrder.Desc })));

            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();
        }

        //BlogIndex
        //BlogTitle : .net 6 ile gelen yenilikler : text, keyword
        //BlogContext : text

        //ProductName : Saç kurutma makinesi text, keyword
        //CustommerFullName : ahmet yıldız : text,keyword

        //Response score değeri ile gelecek.En ilgiliden en ilgisize doğru.

        //GET kibana_sample_data_ecommerce/_search
        //{
        //            "query": {
        //                "match": {
        //                    "category": "shoes clothing"
        //                }
        //            }
        //}
        //Kelime bazlı arama
        public async Task<ImmutableList<ECommerce>> MatchQueryFullTextAsync(string categoryName)
        {
            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
                .Size(100).Query(q => q
                    .Match(m => m
                        .Field(f => f.Category)
                        .Query(categoryName)
                        .Operator(Operator.And))));

            foreach (var hit in result.Hits) 
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();
        }

        //Birden fazla field'a göre arama.
        //Birden fazla field değerinde OR ifadesine göre arama.
        //POST kibana_sample_data_ecommerce/_search
        //{
        //            "query": {
        //                "multi_match": {
        //                    "query": "Sultan",
        //     "fields": ["customer_first_name","customer_full_name", "customer_last_name"]
        //   }
        //            }
        //}
        public async Task<ImmutableList<ECommerce>> MultiMatchQueryFullTextAsync(string name)
        {
            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
                .Size(1000).Query(q => q
                    .MultiMatch(mm =>
                        mm.Fields(new Field("customer_first_name")
                            .And(new Field("customer_last_name"))
                            .And(new Field("customer_full_name")))
                            .Query(name))));

            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();
        }


        //Birden fazla kelime üzerinde arama yaptığınız zaman elasticsearch bu aramayı şöyle çeviriyor.
        //Son kelimeyi bir prefix olarak algılıyor.

        //moranxx..OR sultan OR al

        //POST kibana_sample_data_ecommerce/_search
        //{
        //            "query": {
        //                "match_bool_prefix": {
        //                    "customer_full_name": "Sultan Al Morgan"
        //                }
        //            }
        //        }

        // "Sultan Al M"
        //Rabia Al Barbar
        public async Task<ImmutableList<ECommerce>> MatchBoolPrefixFullTextAsync(string customerFullName)
        {
            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
                .Size(1000).Query(q => q
                    .MatchBoolPrefix(m => m
                        .Field(f => f.CustomerFullName)
                        .Query(customerFullName))));

            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();
        }

        //Öbek kelimeleri aramak istediğimizde.
        //Match Phrase sıralı şekilde arama yapar.
        //İlk Sultan sonra Al sonra Moran

        //POST kibana_sample_data_ecommerce/_search
        //{
        //            "query": {
        //                "match_phrase": {
        //                    "customer_full_name": "Sultan Al Moran"
        //                }
        //            }
        //        }
        //1 data getirdi.

        //Sıralaması önemsiz eşleşen dataları getirir. OR ifadesine göre sıralamasız.

        //Al Mehmet Yıldız
        //Moran Al Çakır

        //POST kibana_sample_data_ecommerce/_search
        //{
        //            "query": {
        //                "match": {
        //                    "customer_full_name": "Sultan Al Moran"
        //                }
        //            }
        // }
        public async Task<ImmutableList<ECommerce>> MatchPhraseFullTextAsync(string customerFullName)
        {
            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
                .Size(1000).Query(q => q
                    .MatchPhrase(m => m
                        .Field(f => f.CustomerFullName)
                        .Query(customerFullName))));

            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();
        }


        //Bir öbek olarak prefix üzerinden arama.

        //Sıralamaya dikkat eder. 3 kelime prefix. Herhangi bir yerde olabilir.

        ////Mehmet Yılmaz Armağan
        ////Mehmet Yılmaz Arm
        ////Ömer Mehmet Yılmaz Armay


        //POST kibana_sample_data_ecommerce/_search
        //{
        //            "query": {
        //                "match_phrase_prefix": {
        //                    "customer_full_name": "Mehmet Yılmaz Armağan"
        //                }
        //            }
        //        }

        //Murtaza Yılmaz ile eşleşir.
        public async Task<ImmutableList<ECommerce>> MatchPhrasePrefixFullTextAsync(string customerFullName)
        {
            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
                .Size(1000).Query(q => q
                    .MatchPhrasePrefix(m => m
                        .Field(f => f.CustomerFullName)
                        .Query(customerFullName))));

            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();
        }


        //must Kullanıldığında içerisindeki sağlayan dataları getirir.Score katkı sağlar. Bulunmak zorunda.
        //filter Eşleşen datalar içerisinde mutlaka gözükmek zorunda.Ama score katkı sağlamaz.
        //should OR gibi davranır. Zorunlu değildir. Döküman içerisinde datalar gözükebilir. Olması score katkı sağlar. Eşleşenleri de getirir eşleşmeyenleri de.
        //must not    Olmasını istemediğimiz dataları getirir. Score katkısı yoktur.

        //GET kibana_sample_data_ecommerce/_search
        //{
        //  "query": {
        //    "bool": {
        //     "must": [
        //       {
        //         "term": {
        //           "geoip.city_name": {
        //             "value": "New York"
        //           }
        //}
        //       }
        //     ], 
        //      "must_not": [
        //        {
        //          "range": {
        //            "taxful_total_price": {
        //              "lte": 100.00
        //            }
        //          }
        //        }
        //      ],
        //      "should": [
        //        {
        //          "term": {
        //            "category.keyword": {
        //              "value": "Women's Clothing"
        //            }
        //          }
        //        }
        //      ],
        //      "filter": [
        //        {
        //          "term": {
        //            "manufacturer.keyword": "Tigress Enterprises"
        //          }
        //        }
        //      ]
        //    }
        //  }
        //}
        public async Task<ImmutableList<ECommerce>> CompoundQueryExampleOneAsync(string cityName, double taxfulTotalPrice, string categoryName, string manufacturer)
        {
            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
                .Size(1000).Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .Term(t => t
                                .Field("geoip.city_name")
                                .Value(cityName)))
                        .MustNot(mn => mn
                            .Range(r => r
                                .NumberRange(nr => nr
                                    .Field(f => f.TaxfulTotalPrice)
                                    .Lte(taxfulTotalPrice))))
                        .Should(s => s.Term(t => t
                            .Field(f => f.Category.Suffix("keyword"))
                            .Value(categoryName)))
                        .Filter(f => f
                            .Term(t => t
                                .Field("manufacturer.keyword")
                                .Value(manufacturer))))
                ));

            foreach (var hit in result.Hits) 
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();
        }


        public async Task<ImmutableList<ECommerce>> CompoundQueryExampleTwoAsync(string customerFullName)
        {

            //var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
            //	.Size(1000).Query(q =>q.MatchPhrasePrefix(m=>m.Field(f=>f.CustomerFullName).Query(customerFullName))));

            var result = await _client.SearchAsync<ECommerce>(s => s.Index(indexName)
                .Size(1000).Query(q => q
                    .Bool(b => b
                        .Should(m => m
                            .Match(m => m
                                .Field(f => f.CustomerFullName)
                                .Query(customerFullName))
                            .Prefix(p => p
                                .Field(f => f.CustomerFullName.Suffix("keyword"))
                                .Value(customerFullName))))));

            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();
        }

    }
}
