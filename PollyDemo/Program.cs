using System;
using System.Net.Http;
using Polly;
using Polly.CircuitBreaker;

namespace PollyDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //1.定义故障 发生ArgumentException会触发策略
            //2.指定策略
            //3.执行策略
            Policy.Handle<ArgumentException>()
                .Fallback(() =>
                {
                    Console.WriteLine("Polly Fallback");
                })
                .Execute(() =>
                {
                    Console.WriteLine("Polly Execute");

                    throw new ArgumentException("hello");
                });


            //  单个异常的故障
            Policy.Handle<Exception>();

            //  带条件的异常类型
            Policy.Handle<ArgumentException>(e => e.Message == "123");

            //  多个异常类型
            Policy.Handle<ArgumentException>()
                .Or<HttpRequestException>()
                .Or<AggregateException>();

            //  多个异常类型带条件
            Policy.Handle<ArgumentException>(e => e.Message == "111")
                .Or<HttpRequestException>(e => e.Message == "111")
                .Or<AggregateException>(e => e.Message == "111");



            //  ----------------------------------------------
            //  重试

            //  默认重试一次
            Policy.Handle<Exception>().Retry();
            //  重试多次
            Policy.Handle<Exception>().Retry(3);
            Policy.Handle<Exception>().Retry(3, (e, i) =>
            {

            });
            //  一直重试
            Policy.Handle<Exception>().RetryForever();
            //  重试且等待
            //Policy.Handle<Exception>().WaitAndRetry();



            //  ----------------------------------------------
            //  断路器
            //  连续触发了(2)次的故障后就开启断路器(OPEN),进入熔断状态，(1)分钟
            Policy.Handle<Exception>().CircuitBreaker(2,
                TimeSpan.FromMinutes(1),
                (e, span) => { },//Open
                () => { }//Close
                );


            var breaker = Policy.Handle<Exception>().CircuitBreaker(2, TimeSpan.FromMinutes(1));
            //断路器有三种状态，OPEN CLOSE HALF-OPEN
            //breaker.CircuitState == CircuitState.Closed;
            //breaker.CircuitState == CircuitState.HalfOpen;
            //breaker.CircuitState == CircuitState.Open;

            //手动开启状态
            //breaker.CircuitState == CircuitState.Islated;

            //手动开启，关闭断路器
            breaker.Isolate();
            breaker.Reset();

            //高级断路器
            //如果在故障采样持续时间内，导致处理异常的操作的比例超过故障阀值，则发生熔断
            //前提是在此期间，通过断路器的操作的数量至少是最小吞吐量
            Policy.Handle<Exception>()
                .AdvancedCircuitBreaker(
                0.5,                      //故障阀值  50%
                TimeSpan.FromSeconds(10), //故障采样时间
                8,                        //最小吞出量
                TimeSpan.FromSeconds(30)  //熔断时间
                );
            //half-open 半开启装态，断路器会尝试着释放（1次）操作，尝试去请求，
            //如果成功，就变成close，如果失败，断路器打开open（30秒）


            //-------------------------------------------------
            //超时  服务调用慢
            //Policy.Timeout(3, (c, s, a, b) =>);


            //-------------------------------------------------
            //舱壁隔离，通过控制并发数量来管理负载,超过12的都拒绝掉
            //50等待
            Policy.Bulkhead(12, 50).Execute(() =>
             {
             });

            //-------------------------------------------------
            //策略包装 策略组合
            var fallback = Policy.Handle<Exception>()
                .Fallback(() =>
                {
                    Console.WriteLine("Polly Fallback");
                });

            var retry = Policy.Handle<Exception>().Retry(3, (e, i) =>
            {
                Console.WriteLine($"retryCount{i}");
            });

            //从右到左
            var policy = Policy.Wrap(fallback, retry);
            policy.Execute(() =>
            {
                Console.WriteLine("Execute");
                throw new Exception("异常");
            });
        }
    }
}
