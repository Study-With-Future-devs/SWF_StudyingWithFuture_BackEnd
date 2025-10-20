SELECT 
    u.Id,
    u.Nome,
    u.Email,
    u.TipoUsuario,
    t.Nome AS Tela,
    t.Descricao AS DescricaoTela
FROM Usuarios u
INNER JOIN UsuarioTelas ut ON u.Id = ut.UsuarioId
INNER JOIN Telas t ON ut.TelaId = t.Id;
