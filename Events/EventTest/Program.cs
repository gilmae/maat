using System;
using Events;

namespace StrangeVanilla.Maat
{
    class Program
    {
        static void Main(string[] args)
        {
            int blog_id = 1;
            MemoryStore<Blog> store = new MemoryStore<Blog>();
            CreateBlog e = new CreateBlog { Id = blog_id, Content = "Test" };
            store.StoreEvent(e);

            Blog b = new Blog();

            b = store.Retrieve(blog_id, new Blog());



            Console.WriteLine("Hello World!");
        }
    }
}
