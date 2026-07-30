using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSW.Helpers
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 优化的 Skip，针对 Enumerable.Repeat 生成的序列，避免迭代。
        /// </summary>
        public static IEnumerable<T> SkipFast<T>(this IEnumerable<T> source, int count)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (count <= 0) return source;

            // 尝试获取内部 RepeatIterator 的元素和重复次数
            if (TryGetRepeatIteratorInfo(source, out T element, out long totalCount))
            {
                long remaining = totalCount - count;
                if (remaining <= 0)
                    return Enumerable.Empty<T>();
                // 剩余部分仍然是重复相同元素
                return Enumerable.Repeat(element, (int)remaining);  // 注意：这里需要确保 remaining <= int.MaxValue
            }

            // 回退到标准 Skip（处理其他 IEnumerable<T>）
            return source.Skip(count);
        }

        private static bool TryGetRepeatIteratorInfo<T>(IEnumerable<T> source, out T element, out long totalCount)
        {
            element = default!;
            totalCount = 0;

            // Enumerable.Repeat 生成的迭代器内部类型是 System.Linq.Enumerable+RepeatIterator<T>
            var type = source.GetType();
            if (type.FullName?.StartsWith("System.Linq.Enumerable+RepeatIterator`1") == true)
            {
                // 通过反射获取私有字段 _element 和 _count
                var elementField = type.GetField("_element", BindingFlags.NonPublic | BindingFlags.Instance);
                var countField = type.GetField("_count", BindingFlags.NonPublic | BindingFlags.Instance);
                if (elementField != null && countField != null)
                {
                    element = (T)elementField.GetValue(source);
                    totalCount = (int)countField.GetValue(source); // _count 是 int 类型
                    return true;
                }
            }
            return false;
        }
    }
}
