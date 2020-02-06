﻿using Database;
using Entities;
using IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repositories
{
    public class BookRepository : BaseRepository<Book>, IBookRepository
    {
        private readonly BookStoreContext _context;
        private readonly IAuthorRepository _authorRepository;

        //Inyectamos el contexto (es decir, la base de datos para que entity Framework la pueda usar acá)
        public BookRepository(BookStoreContext context, IAuthorRepository authorRepository) : base(context)
        {
            _context = context;
            _authorRepository = authorRepository;
        }

        public override Book GetById(object id)
        {
            return _context.Books.Where(b => b.Id == (Guid)id)
                .Include(b => b.Author)
                .Include(b => b.BookClients)
                .ThenInclude(bc => bc.Client)
                .FirstOrDefault();
               
        }

        public override IList<Book> GetAll()
        {
            //Los includes lo que hacen es traer datos anidados que no estén directo en la tabla Books (como hacemos con los inner join en SQL)
            //Entonces acá decimos, Traeme los libros que esten en nuestro contexto (base de datos) y también incluime los BookClients hijos y los Clients de esos BookClients
            return _context.Books
                .Include(b => b.Author)
                .Include(b => b.BookClients)
                .ThenInclude(bc => bc.Client)
                .ToList();
        }

        public override void Insert(Book obj)
        {
            var book = obj;
            var author = _authorRepository.GetById(book.Author.Id);
            book.Author = author;
            _context.Add(book);
            _context.SaveChanges();
        }

        public int Alquilar(Guid id)
        {
            var book = GetById(id);
            book.Stock -= 1;
            Update(book);
            return book.Stock;
        }
        public void Devolver(Guid id)
        {
            var book = GetById(id);
            book.Stock += 1;
            Update(book);

        }
    }
}
