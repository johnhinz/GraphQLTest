using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GraphQL;
using GraphQL.Types;
using Newtonsoft.Json.Linq;

namespace GraphQLTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GraphQLQuery query)
        {
            var inputs = query.Variables.ToInputs();
            var schema = new Schema()
            {
                Query = new CustomerQuery()
            };

            var result = await new DocumentExecuter().ExecuteAsync(_ =>
            {
                _.Schema = schema;
                _.Query = query.Query;
                _.OperationName = query.OperationName;
                _.Inputs = inputs;
            });

            return Ok(result);
        }
    }

    public class GraphQLQuery
    {
        public string OperationName { get; set; }
        public string NamedQuery { get; set; }
        public string Query { get; set; }
        public JObject Variables { get; set; }
    }

    public class CustomerQuery : ObjectGraphType
    {
        public CustomerQuery()
        {
            Field<CustomerGraphqlType>(
                "CustomerInfo",
                resolve: context => { return getCustomer(); }
            );

        }

        public Customer getCustomer()
        {
            var purchase = new Purchase() { prodId = 1, prodName = "A" };
            List<Purchase> p = new List<Purchase>();
            p.Add(purchase);

            return new Customer { firstName = "Elon", lastName = "Musks", Purchases = p };

        }

        public class CustomerGraphqlType : ObjectGraphType<Customer>
        {
            public CustomerGraphqlType()
            {
                Field(x => x.firstName).Description("customer first name");
                Field(x => x.lastName).Description("customer last name");
                Field(x => x.Purchases, type: typeof(ListGraphType<PurchaseGraphqlType>)).Description("Purchase list");
            }


        }

        public class PurchaseGraphqlType : ObjectGraphType<Purchase>
        {
            public PurchaseGraphqlType()
            {
                Name = "PurchaseGraphqlType";
                Field(x => x.prodId).Description("Product code");
                Field(x => x.prodName).Description("Pruchase Desc");
            }
        }

        public class Customer
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public List<Purchase> Purchases { get; set; }
        }

        public class Purchase
        {
            public int prodId { get; set; }
            public string prodName { get; set; }
        }
    }
}