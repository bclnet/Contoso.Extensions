//using System.Threading.Tasks;

//namespace Microsoft.Extensions.Caching.FileSystem
//{
//    internal static class FileSystemExtensions
//    {
//        internal static RedisValue[] HashMemberGet(this IDatabase cache, string key, params string[] members)
//        {
//            var result = cache.ScriptEvaluate(
//                HmGetScript,
//                new RedisKey[] { key },
//                GetRedisMembers(members));

//            // TODO: Error checking?
//            return (RedisValue[])result;
//        }

//        internal static async Task<RedisValue[]> HashMemberGetAsync(this IDatabase cache, string key, params string[] members)
//        {
//            var result = await cache.ScriptEvaluateAsync(
//                HmGetScript,
//                new RedisKey[] { key },
//                GetRedisMembers(members)).ConfigureAwait(false);

//            // TODO: Error checking?
//            return (RedisValue[])result;
//        }

//        static RedisValue[] GetRedisMembers(params string[] members)
//        {
//            var redisMembers = new RedisValue[members.Length];
//            for (var i = 0; i < members.Length; i++)
//                redisMembers[i] = (RedisValue)members[i];
//            return redisMembers;
//        }
//    }
//}