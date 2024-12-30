﻿using Microsoft.EntityFrameworkCore;
using Server.Models;
using Server.Repositories.Base;
using System.Text;

namespace Server.Repositories;

public class ReviewRepository : Repository<Review, int>
{
    public ReviewRepository(FarminhouseContext context) : base(context) { }

    public async Task<ICollection<Review>> GetAllWithFullDataAsync()
    {
        return await GetQueryable()
            .Include(review => review.Product)
            .Include(review => review.User)
            .ToArrayAsync();
    }

    public async Task<Review> GetById(int id)
    {
        // "FirstOrDefaultAsync" DEVUELVE NULO SI NO EXISTE
        Review review = await GetQueryable()
            .Include(review => review.Product)
            .Include(review => review.User)
            .FirstOrDefaultAsync(review => review.Id == id);
        return review;
    }

    public async Task<IEnumerable<Review>> GetByProductIdAsync(int id)
    {
        IEnumerable<Review> reviewsByProductId = await GetQueryable()
            .Where(review => review.ProductId == id)
            .ToListAsync();

        return reviewsByProductId;
    }

    public async Task<IEnumerable<Review>> GetByUserIdAsync(int id)
    {
        IEnumerable<Review> reviewsByUserId = await GetQueryable()
            .Where(review => review.UserId == id)
            .ToListAsync();

        return reviewsByUserId;
    }

    public string DeleteAcents(string text)
    {
        string acents = "áéíóúÁÉÍÓÚ";
        string withOutAcents = "aeiouAEIOU";
        StringBuilder textWithOutAcents = new StringBuilder(text);

        for (int i = 0; i < acents.Length; i++)
        {
            textWithOutAcents.Replace(acents [i], withOutAcents [i]);
        }

        return textWithOutAcents.ToString();
    }
}
