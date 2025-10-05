static class Category {

  public static IResult Create(
    Auth auth, SqliteConnection conn, JsonElement o
  ) {
    if(!auth.IsAdmin()) {
      return Results.BadRequest(new {error = "verboten"});
    }

    if(o.ValueKind is not JsonValueKind.Array) {
      return Results.BadRequest(new {error = "not an array"});
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "insert or ignore into category(name, color) values ";

    int _i = 0;
    foreach(var ob in o.EnumerateArray()) {
      string? name = ob._str("name");
      string? color = ob._str("color") is string c && c.StartsWith("#")
        ? c.Substring(1) : ob._str("color");

      if((name, color) is (null, null)) {
        continue;
      }

      int i = _i++;
      cmd.CommandText += $"(:name_{i}, :color_{i}),";
      cmd.Parameters.AddWithValue($"name_{i}", name);
      cmd.Parameters.AddWithValue($"color_{i}", color);
    }

    if(_i == 0) {
      return Results.BadRequest(new {error = "array is empty"});
    }

    cmd.CommandText = cmd.CommandText.TrimEnd(',');
    cmd.ExecuteNonQuery();

    return Results.Ok();
  }

  public static IResult List(
    Auth auth, SqliteConnection conn, JsonElement? o
  ) {
    if(!auth.IsAdmin()) {
      return Results.BadRequest(new {error = "verboten"});
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "select id, name, color from category where 1 ";

    if(o?._long("id") is long id) {
      cmd.CommandText += " and id=:id ";
      cmd.Parameters.AddWithValue("id", id);
    }

    if(o?._str("name") is string name) {
      cmd.CommandText += " and name like :name ";
      cmd.Parameters.AddWithValue("name", $"%{name}%");
    }

    if(o?._str("color") is string color) {
      cmd.CommandText += " and color = :color ";
      cmd.Parameters.AddWithValue("color", color);
    }

    return Results.Ok(cmd.ExecuteReader().ToDictArray());
  }

  public static IResult Update(
    Auth auth, SqliteConnection conn, JsonElement o
  ) {
    if(!auth.IsAdmin()) {
      return Results.BadRequest(new {error = "verboten"});
    }

    long? id = o._long("id");
    if(id is null) {
      return Results.BadRequest(new {error = "need an id"});
    }

    string? name = o._str("name");
    string? color = o._str("color");

    if((name, color) is (null, null)) {
      return Results.BadRequest(new {error = "no field to update"});
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "update or ignore category set ";

    if(name is not null) {
      cmd.CommandText += " name = :name ,";
      cmd.Parameters.AddWithValue("name", name);
    }

    if(color is not null) {
      cmd.CommandText += " color = :color ,";
      cmd.Parameters.AddWithValue("color",
        color.StartsWith("#") ? color.Substring(1): color
      );
    }

    cmd.CommandText = cmd.CommandText.TrimEnd(',');

    cmd.CommandText += " where id = :id";
    cmd.Parameters.AddWithValue("id", id);

    cmd.ExecuteNonQuery();

    return Results.Ok();
  }

  public static IResult Delete(
    Auth auth, SqliteConnection conn, JsonElement o
  ) {
    if(!auth.IsAdmin()) {
      return Results.BadRequest(new {error = "verboten"});
    }

    long? id = o._long("id");
    if(id is null) {
      return Results.BadRequest(new {error = "need an id"});
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "delete from category where id = :id";
    cmd.Parameters.AddWithValue("id", id);
    cmd.ExecuteNonQuery();

    return Results.Ok();
  }

}
