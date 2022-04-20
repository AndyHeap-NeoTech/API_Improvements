using System;
using System.Threading.Tasks;

namespace API_Improvements
{
    class Program
    {   

        static async Task Main(string[] args)
        {
            var driver = new Driver();
            var session = driver.Session();
            
            //Simplest usage would become this...
            await session.Run("My single query").ExecuteAsync();


            //Then parameters...
            await session.Run("my query $param1", 
                    new {param1 = "param1"}, 
                    AccessMode.Smart)
                .ExecuteAsync();


            
            //Multiple queries in a single transaction...   
            await session.Run("My Query")
                .Then("Next Query")
                .Then("Final Query")
                .ExecuteAsync(); //Not convinced by adding this


            
            //Single query with result handling using a lambda
            //Note Lambda simplified as there is no transaction or access mode exposure to user
            IRecord queryRecord;
            await session.Run("Result handling query", 
                    null,
                    async r =>
                    {
                        while (await r.FetchAsync())
                        {
                            queryRecord = r.Current();
                            //do something with the records contents here
                            Console.WriteLine(queryRecord.Contents);
                        }
                    },
                    AccessMode.Smart)
                .ExecuteAsync();


            
            //Multiple queries some with result handling using delegate
            await session.Run("Simple write only query", AccessMode.Smart)
                .Then("Query with results $name", 
                    new {name = "Andy"}, 
                    HandleMyResults)
                .Then("Final query")
                .ExecuteAsync();



            //Playing around encapsulating with a class. This feeds results from first query into the second.
            var work = new WorkUnit();
            await work.Run(session);
        }



        static async Task HandleMyResults(IResultCursor resultCursor)
        {
            while (await resultCursor.FetchAsync())
            {
                var queryRecord = resultCursor.Current();
                //do something with the records contents here
                Console.WriteLine(queryRecord.Contents);
            }
        }



        class WorkUnit
        {
            private string FirstResult { get; set; }
            private string SecondResult { get; set; }

            async Task HandleFirstResults(IResultCursor resultCursor)
            {
                while (await resultCursor.FetchAsync())
                {
                    var queryRecord = resultCursor.Current();
                    //do something with the records contents here
                    FirstResult = queryRecord.Contents;
                }
            }

            async Task HandleSecondResults(IResultCursor resultCursor)
            {
                while (await resultCursor.FetchAsync())
                {
                    var queryRecord = resultCursor.Current();
                    //do something with the records contents here
                    SecondResult = queryRecord.Contents;
                }
            }

            public async Task Run(ISession session)
            {
                await session.Run("First query",
                        null,
                        HandleFirstResults,
                        AccessMode.Smart)
                    .Then("Second query using first results $firstResult",
                        new { firstResult = FirstResult },
                        HandleSecondResults)
                    .ExecuteAsync();
            }
        }
    }
}
