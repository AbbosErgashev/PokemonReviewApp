using Microsoft.EntityFrameworkCore;
using PokemonReviewApp.Data;
using PokemonReviewApp.Models;
using PokemonReviewApp.Repositories;
using Xunit;

namespace PokemonReviewApp.Tests;

public class PokemonRepositoryTests
{
    private DataContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new DataContext(options);
    }

    [Theory]
    [InlineData(0, 0)] // No reviews should return 0
    [InlineData(1, 4, 4)] // Single review with rating 4 should return 4
    [InlineData(2, 3, 5, 4)] // Two reviews with ratings 3,5 should return average 4
    [InlineData(3, 2, 4, 6, 4)] // Three reviews with ratings 2,4,6 should return average 4
    public void GetPokemonRating_WithDifferentReviewCounts_ReturnsCorrectRating(int reviewCount, params int[] ratings)
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new PokemonRepository(context);

        var pokemon = new Pokemon
        {
            Id = 1,
            Name = "Test Pokemon",
            BirthDate = DateTime.Now,
            Reviews = new List<Review>()
        };

        context.Pokemon.Add(pokemon);

        // Add reviews based on the test data
        for (int i = 0; i < reviewCount; i++)
        {
            var review = new Review
            {
                Id = i + 1,
                Title = $"Review {i + 1}",
                Text = $"Review text {i + 1}",
                Rating = ratings[i],
                Pokemon = pokemon
            };
            context.Reviews.Add(review);
            pokemon.Reviews.Add(review);
        }

        context.SaveChanges();

        decimal expectedRating = reviewCount <= 0 ? 0 : (decimal)ratings.Take(reviewCount).Average();

        // Act
        var result = repository.GetPokemonRating(1);

        // Assert
        Assert.Equal(expectedRating, result);
    }

    [Theory]
    [InlineData(1, true)] // SaveChanges returns 1, should return true
    [InlineData(2, true)] // SaveChanges returns 2, should return true
    [InlineData(0, false)] // SaveChanges returns 0, should return false
    [InlineData(-1, false)] // SaveChanges returns -1, should return false
    public void Save_WithDifferentSaveResults_ReturnsCorrectBooleanValue(int saveChangesResult, bool expectedResult)
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new PokemonRepository(context);

        // Add some data to ensure SaveChanges can return the expected result
        if (saveChangesResult > 0)
        {
            var pokemon = new Pokemon
            {
                Name = "Test Pokemon",
                BirthDate = DateTime.Now,
                Reviews = new List<Review>(),
                PokemonOwners = new List<PokemonOwner>(),
                PokemonCategories = new List<PokemonCategory>()
            };
            context.Pokemon.Add(pokemon);
        }

        // Act
        var result = repository.Save();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void GetPokemonRating_WhenNoReviewsExist_ReturnsZero()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new PokemonRepository(context);

        var pokemon = new Pokemon
        {
            Id = 1,
            Name = "Test Pokemon",
            BirthDate = DateTime.Now,
            Reviews = new List<Review>()
        };

        context.Pokemon.Add(pokemon);
        context.SaveChanges();

        // Act
        var result = repository.GetPokemonRating(1);

        // Assert - This specifically tests the if (review.Count() <= 0) condition
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetPokemonRating_WhenReviewsExist_ReturnsAverageRating()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new PokemonRepository(context);

        var pokemon = new Pokemon
        {
            Id = 1,
            Name = "Test Pokemon",
            BirthDate = DateTime.Now,
            Reviews = new List<Review>()
        };

        context.Pokemon.Add(pokemon);

        var review1 = new Review { Id = 1, Title = "Good", Text = "Nice", Rating = 4, Pokemon = pokemon };
        var review2 = new Review { Id = 2, Title = "Great", Text = "Awesome", Rating = 5, Pokemon = pokemon };

        context.Reviews.AddRange(review1, review2);
        context.SaveChanges();

        // Act
        var result = repository.GetPokemonRating(1);

        // Assert - This tests the else part of the if condition (review.Count() > 0)
        Assert.Equal(4.5m, result);
    }
}