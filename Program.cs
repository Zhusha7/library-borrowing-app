using System.Data.SQLite;
using static library_borrowing_app.Program;

namespace library_borrowing_app
{
    internal class Program
    {
        public class Book
        {
            public string Title { get; set; }
            public string Author { get; set; }
            public string Genre { get; set; }
            public bool IsAvailable { get; set; }

            public Book(string title, string author, string genre)
            {
                Title = title;
                Author = author;
                Genre = genre;
                IsAvailable = true;
            }
            public Book(string title, string author, string genre, bool isAvailable)
            {
                Title = title;
                Author = author;
                Genre = genre;
                IsAvailable = isAvailable;
            }
        }

        public class Borrower
        {
            public string Name { get; set; }
            public string ContactInformation { get; set; }
            public List<Book> BorrowedBooks { get; set; }

            public Borrower(string name, string contactInformation)
            {
                Name = name;
                ContactInformation = contactInformation;
                BorrowedBooks = new List<Book>();
            }
        }

        public class Transaction
        {
            public Book Book { get; set; }
            public Borrower Borrower { get; set; }
            public DateTime TransactionDate { get; set; }

            public Transaction(Book book, Borrower borrower, DateTime transactionDate)
            {
                Book = book;
                Borrower = borrower;
                TransactionDate = transactionDate;
            }
        }

        public class Library
        {
            private List<Book> _books;
            private List<Borrower> _borrowers;
            private List<Transaction> _transactions;
            private SQLiteConnection con;
            private SQLiteCommand cmd;
            private SQLiteDataReader reader;
            public Library()
            {
                _books = new List<Book>();
                _borrowers = new List<Borrower>();
                _transactions = new List<Transaction>();
                const string cs = @"URI=file:C:\Dev\dev-cs\library-borrowing-app\library.db";

                con = new SQLiteConnection(cs);
                con.Open();

                cmd = new SQLiteCommand(con);
                
                cmd.CommandText = "SELECT * FROM library";
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _books.Add(new Book(reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetBoolean(4)));
                }
                reader.Close();
            }

            public void AddBook(Book book)
            {
                _books.Add(book);
                cmd.CommandText =
                    "INSERT INTO library(title, author, genre, availability) VALUES(@title, @author, @genre, @available)";
                cmd.Parameters.AddWithValue("@title", book.Title);
                cmd.Parameters.AddWithValue("@author", book.Author);
                cmd.Parameters.AddWithValue("@genre", book.Genre);
                cmd.Parameters.AddWithValue("@available", book.IsAvailable);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }

            public void RemoveBook(Book book)
            {
                if (_books.Remove(book))
                {
                    cmd.CommandText = $"DELETE FROM library WHERE title = '{book.Title}' AND author = '{book.Author}' AND title = '{book.Genre}'";
                    cmd.ExecuteNonQuery();
                }
            }
            
            public void RegisterBorrower(Borrower borrower)
            {
                _borrowers.Add(borrower);
            }

            public void BorrowBook(Book book, Borrower borrower)
            {
                if (!book.IsAvailable)
                {
                    Console.WriteLine("The book is not available for borrowing.");
                    return;
                }

                book.IsAvailable = false;
                borrower.BorrowedBooks.Add(book);
                _transactions.Add(new Transaction(book, borrower, DateTime.Now));
            }

            public List<Book> SearchBooks(string keyword)
            {
                keyword = keyword.ToLower();
                List<Book> searchResults = new List<Book>();

                foreach (Book book in _books)
                {
                    if (book.Title.ToLower().Contains(keyword) ||
                        book.Author.ToLower().Contains(keyword) ||
                        book.Genre.ToLower().Contains(keyword))
                    {
                        searchResults.Add(book);
                    }
                }

                return searchResults;
            }

            public void DisplayAllBooks()
            {
                reader = cmd.ExecuteReader();
                Console.WriteLine($"{reader.GetName(1),-18} {reader.GetName(2),-24} {reader.GetName(3),-10} {reader.GetName(4)}");
                while (reader.Read())
                {
                    string availability = reader.GetBoolean(4) ? "Available" : "Borrowed";
                    Console.WriteLine(
                        $@"{reader.GetString(1),-18} {reader.GetString(2),-24} {reader.GetString(3),-10} {availability}");
                }
                reader.Close();
            }

            public void DisplayBorrowersAndBooks()
            {
                foreach (Borrower borrower in _borrowers)
                {
                    Console.WriteLine($"Borrower: {borrower.Name}, Contact Information: {borrower.ContactInformation}");
                    Console.WriteLine("Borrowed Books:");

                    if (borrower.BorrowedBooks.Count == 0)
                    {
                        Console.WriteLine("No books borrowed.");
                    }
                    else
                    {
                        foreach (Book book in borrower.BorrowedBooks)
                        {
                            Console.WriteLine($"Title: {book.Title}, Author: {book.Author}, Genre: {book.Genre}");
                        }
                    }

                    Console.WriteLine();
                }
            }
        }
        

        static void Main(string[] args)
        {
            Library library = new();
            
            library.DisplayAllBooks();
        }
    }
}