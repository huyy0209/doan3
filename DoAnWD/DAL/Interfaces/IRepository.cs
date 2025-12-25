using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DoAnWD.DAL.Interfaces
{
    /// <summary>
    /// Generic Repository Pattern Interface
    /// OOP: Abstraction - Định nghĩa contract chung cho tất cả repositories
    /// </summary>
    /// <typeparam name="T">Entity type (GiaSu, HocVien, LopHoc...)</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// CREATE - Thêm entity mới
        /// </summary>
        void Add(T entity);

        /// <summary>
        /// READ - Lấy entity theo ID
        /// </summary>
        T GetById(int id);

        /// <summary>
        /// READ - Lấy tất cả entities
        /// </summary>
        IEnumerable<T> GetAll();

        /// <summary>
        /// READ - Tìm theo điều kiện (LINQ Expression)
        /// </summary>
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// UPDATE - Cập nhật entity
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// DELETE - Xóa một entity
        /// </summary>
        void Remove(T entity);

        /// <summary>
        /// DELETE - Xóa nhiều entities
        /// </summary>
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>
        /// SAVE - Lưu thay đổi vào database
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Đếm số lượng entities
        /// </summary>
        int Count();

        /// <summary>
        /// Đếm số lượng theo điều kiện
        /// </summary>
        int Count(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Kiểm tra tồn tại theo điều kiện
        /// </summary>
        bool Any(Expression<Func<T, bool>> predicate);
    }
}