using System;
using System.Collections.Generic; //Req. for list
using System.Linq; //Req. for LINQ query (FirstOrDefault)
using Microsoft.AspNetCore.MVC; //Req. for building APIs

namespace Server
{
    internal class API
    {
    }
}

public class Category
{ 
    public int Cid { get; set; }
    public string Name { get; set; }
}

//API request controller

public class CategoriesController : ControllerBase
{
    private static List<Category> _categories = new List<Category>();
    {
        new Category 

    }:

}