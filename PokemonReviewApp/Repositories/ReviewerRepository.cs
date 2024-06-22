using Microsoft.EntityFrameworkCore;
using PokemonReviewApp.Data;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Repositories
{
    public class ReviewerRepository : IReviewerRepository
    {
        private readonly DataContext _context;

        public ReviewerRepository(DataContext context)
            => _context = context;


        public ICollection<Review> GetReviewByReviewer(int reviewerId)
            => _context.Reviews.Where(r => r.Reviewer.Id == reviewerId).ToList();

        public Reviewer GetReviewer(int reviewerId)
            => _context.Reviewers.Where(r => r.Id == reviewerId)
            .Include(e => e.Reviews).FirstOrDefault();


        public ICollection<Reviewer> GetReviewers()
            => _context.Reviewers.ToList();

        public bool ReviewerExists(int reviewerId)
            => _context.Reviewers.Any(r => r.Id == reviewerId);
    }
}
