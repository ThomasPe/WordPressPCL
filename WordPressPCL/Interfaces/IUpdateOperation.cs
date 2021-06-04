using System.Threading.Tasks;

namespace WordPressPCL.Interfaces
{
    /// <summary>
    /// Interface with required Update methods
    /// </summary>
    /// <typeparam name="TClass">return class type</typeparam>
    public interface IUpdateOperation<TClass>
    {
        /// <summary>
        /// Update entity method
        /// </summary>
        /// <param name="Entity">object to update</param>
        /// <returns>Updated entity</returns>
        Task<TClass> UpdateAsync(TClass Entity);
    }
}