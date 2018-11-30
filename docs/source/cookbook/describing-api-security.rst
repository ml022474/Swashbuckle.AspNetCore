Describing API Security
=======================

To describe API security in an OpenAPI document, you define one or more supported schemes (e.g. basic, api key, oauth2 etc.) with the ``securitySchemes`` keyword, and then you specify which of those schemes are applicable with the ``security`` keyword, which can be applied globally or on individual operations. To learn more about describing security in an OpenAPI document, checkout out the `OpenAPI docs here <https://swagger.io/docs/specification/describing-parameters/>`_.

In Swashbuckle, you can define schemes by invoking the `AddSecurityDefinition` method, providing a name and an instance of `OpenApiSecurityScheme`. For example you can define an [OAuth 2.0 - implicit flow](https://oauth.net/2/) as follows:
To describe API security in an OpenAPI document, you use the ``securitySchemes`` keyword to list the scheme(s) your API supports, and the 
