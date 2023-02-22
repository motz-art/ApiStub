# ApiStub

A small REST API stub for prompt UI development.

## Quick start

Create `products.json` file in `data` directory. E.g.:

``` JSON
	[
		{ "id": 1, "name": "apple", "price": 2.69, "group": "fruites" },
		{ "id": 2, "name": "bananas", "price": 1.39, "group": "fruites" },
		{ "id": 3, "name": "cucumber", "price": 1.39, "group": "vegitables" },
	]
```
**Note**: file must contain well formatted JSON array as a single root element.

Check `appsetting.json` and put absolut path to `data` directory into `DataDirectory` property.

Run:

``` Bash
    dotnet run
```

### Querying

To query all products make a `GET` request to `/api/products`. **Note**: URI path matches next format: `"api/{name}"` where `name` should match corresponding file name in a `data` directory. If file does not exists, it will return empty array.

API stub will return:
``` JSON
	{
		"items": [
			{ "id": 1, "name": "apple", "price": 2.69, "group": "fruites" },
			{ "id": 2, "name": "bananas", "price": 1.39, "group": "fruites" },
			{ "id": 3, "name": "cucumber", "price": 1.39, "group": "vegitables" },
		],
		"totalItems": 3
	}
```

To get only items where `group` equals `fruites` request `/api/products?group=fruites`

To get maximum 5 items use `limit` query parameter set to `5`.

To start at specific item use `skip` query parameter (zero based).

For example: `/api/products?group=fruites&skip=1&limit=5` will return:
``` JSON
	{
		"items": [
			{ "id": 2, "name": "bananas", "price": 1.39, "group": "fruites" },
		],
		"totalItems": 2
	}
```

Note how `totalItems` changes when using filters. This allows to easily implement ant test paging.

### Creating new items

To add new item send `POST` request to `api/products` with item you would like to add in the request body. For example:

``` JSON
	// POST api/products

	{
		"name": "orange", "price": 0.79, "group": "fruites" 
	}
```

API will response:

``` JSON
	{
		"id": 4, "name": "orange", "price": 0.79, "group": "fruites" 
	}
```

**Note**: note how API Stub auto generates `id`'s!

**Note**: Any state change happens in memory only and will be reset to initial state after restart. Data `.json` files will remain unchanged.
### Updating items

To update item send `PUT` request to `api/products/{id}` with update item data in the request body. For example:

``` JSON
	// PUT api/products/3

	{
		"name": "onion", "price": 0.79, "group": "vegitables" 
	}
```

API will response:

``` JSON
	{
		"id": 3, "name": "onion", "price": 0.79, "group": "vegitables"
	}
```

**Note**: Any state change happens in memory only and will be reset to initial state after restart. Data `.json` files will remain unchanged.
### Removing items

To remove any item send `DELETE` request to `api/products/{id}`. For example:

``` HTTP
	> DELETE api/products/4
```

API will always response with status code `200 OK` even if item with specified id doesn't exists.

**Note**: Any state change happens in memory only and will be reset to initial state after restart. Data `.json` files will remain unchanged.