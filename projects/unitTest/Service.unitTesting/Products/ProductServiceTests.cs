

using Core.CrossCuttingConcerns.Exceptions;
using DataAccess.Repositories.Abstracts;
using Models.Dtos.RequestDto;
using Models.Dtos.ResponseDto;
using Models.Entities;
using Moq;
using Service.BusinessRules;
using Service.BusinessRules.Abstract;
using Service.Concrete;
using System.Net;

namespace Service.unitTesting.Products;

public class ProductServiceTests
{
    private ProductService _service;
    private Mock<IProductRepository> _mockRepository;
    private Mock<IProductRules> _mockRules;

    private ProductAddRequest productAddRequest;
    private ProductUpdateRequest productUpdateRequest;
    private Product product;
    private ProductResponseDto productResponseDto;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockRules = new Mock<IProductRules>();
        _service = new ProductService(_mockRepository.Object, _mockRules.Object);
        productAddRequest = new ProductAddRequest(Name: "Test", Stock: 25, Price: 2500, CategoryId: 1);
        productUpdateRequest = new ProductUpdateRequest(Id: new Guid(), Name: "Test", Stock: 25, Price: 2500, CategoryId: 1);
        product = new Product
        {
            Id = new Guid(),
            Name = "Test",
            CategoryId = 1,
            Price = 2500,
            Stock = 25,
            Category = new Category() { Id = 1, Name = "Teknoloji", Products = new List<Product>() { new Product() } }
        };

        productResponseDto = new ProductResponseDto(Id: new Guid(), Name: "Test", Stock: 25, Price: 2500, CategoryId: 1);
    }
    [Test]
    public void Add_WhenProductNameIsUnique_ReturnsOk()
    {
        _mockRules.Setup(_mockRules => _mockRules.ProductNameMustBeUnique(product.Name)).Verifiable();

        var result = _service.Add(productAddRequest);

        Assert.AreEqual(result.StatusCode, HttpStatusCode.Created);
        Assert.AreEqual(result.Data, productResponseDto);
        Assert.AreEqual(result.Message, "Ürün Eklendi");

    }

    [Test]
    public void Add_WhenProductNameIsNotUnique_ReturnsBadRequest()
    {
        _mockRules.Setup(_mockRules => _mockRules.ProductNameMustBeUnique(product.Name)).Throws(new BusinessException("Ürün ismi benzersiz olmalı")).Verifiable();

        var result = _service.Add(productAddRequest);

        Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
        Assert.AreEqual(result.Message, "Ürün ismi benzersiz olmalı");

    }

    [Test]
    public void
        Delete_WhenProductIsPresent_ReturnsOk()
    {
        //Arrange
        Guid Id = new Guid();

        _mockRules.Setup(result => result.ProductIsPresent(Id));
        _mockRepository.Setup(result => result.GetById(Id,null)).Returns(product);
        _mockRepository.Setup(result => result.Delete(product));

        //Act
        var result = _service.Delete(Id);

        //Assert
        Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
        Assert.AreEqual(result.Message, "Ürün Silindi");
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(Id, result.Data.Id);


    }
    [Test]
    public void
       Delete_WhenProductIsPresent_ReturnsBadRequest()
    {
        //Arrange
        Guid Id = Guid.NewGuid();

        _mockRules.Setup(result => result.ProductIsPresent(Id)).Throws(new BusinessException($"Id si : {Id} olan ürün bulunamadı."));
        _mockRepository.Setup(result => result.GetById(Id, null)).Returns(product);
        _mockRepository.Setup(result => result.Delete(product));

        //Act
        var result = _service.Delete(Id);

        //Assert
        Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
        Assert.AreEqual(result.Message, $"Id si : {Id} olan ürün bulunamadı.");
    }
}
