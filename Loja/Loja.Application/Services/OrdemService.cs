﻿using AutoMapper;
using Loja.Application.DTOs;
using Loja.Application.Interfaces;
using Loja.Domain.Entities;
using Loja.Domain.Enums;
using Loja.Domain.Interfaces;
using Loja.Domain.PaginationEntities;
using System.Linq.Expressions;

namespace Loja.Application.Services
{
    public class OrdemService : IOrdemService
    {
        private readonly IOrdemRepository _ordemRepository;
        private readonly IMapper _mapper;
        public OrdemService(IMapper mapper, IOrdemRepository ordemRepository)
        {
            _ordemRepository = ordemRepository ??
                throw new ArgumentNullException(nameof(ordemRepository));

            _mapper = mapper;

        }

        /// <summary>
        /// Obtém uma ordem pelo Id.
        /// </summary>
        /// <param name="id">Id da Ordem.</param>
        /// <returns>Retorna uma OrdemDTO.</returns>
        public async Task<OrdemDTO> GetById(int? id)
        {
            var ordemEntity = await _ordemRepository.GetByIdAsync(x => x.Id == id);
            return _mapper.Map<OrdemDTO>(ordemEntity);
        }

        /// <summary>
        /// Obtém uma ordem pelo Id.
        /// </summary>
        /// <param name="id">Id da Ordem.</param>
        /// <returns>Retorna uma Ordem com os Produtos inclusos.</returns>
        public async Task<OrdemDTO> GetOrdemComProdutosById(int? id)
        {
            var ordemEntity = await _ordemRepository.GetOrdemByIdIncluiProdutoAsync(x => x.Id == id);
            return _mapper.Map<OrdemDTO>(ordemEntity);
        }

        /// <summary>
        /// Obtém uma lista paginada com as Ordens.
        /// </summary>
        /// <param name="parameters">Objeto com os parâmetros de paginação</param>
        /// <returns>Retorna uma tupla com uma lista de Ordens e os dados de paginação</returns>
        public async Task<Tuple<IList<OrdemDTO>, PagingInfo>> GetOrdens(PagingParameters parameters)
        {
            var orderByExpression = SwitchCaseOrderedBy(parameters.OrderedBy);

            var (ordens, pagingInfo) = await _ordemRepository.GetAsync(parameters, orderByExpression);

            var ordensDto = _mapper.Map<List<OrdemDTO>>(ordens);

            return new Tuple<IList<OrdemDTO>, PagingInfo>(ordensDto, pagingInfo);
        }

        /// <summary>
        /// Obtém uma lista paginada com as Ordens.
        /// <br>Pode ser filtrada pelo Id de vendedor e/ou cliente.</br>
        /// </summary>
        /// <param name="parameters">Objeto com os parâmetros de paginação</param>
        /// <param name="predicate">Delegate com o critério de busca</param>
        /// <returns>Retorna uma tupla com uma lista de Ordens e os dados de paginação</returns>
        public async Task<Tuple<IList<OrdemDTO>, PagingInfo>> GetOrdens(PagingParameters parameters, Expression<Func<Ordem, bool>> predicate)
        {
            var orderByExpression = SwitchCaseOrderedBy(parameters.OrderedBy);

            var (ordens, pagingInfo) = await _ordemRepository.GetAsync(parameters, orderByExpression);

            var ordensDto = _mapper.Map<List<OrdemDTO>>(ordens);

            return new Tuple<IList<OrdemDTO>, PagingInfo>(ordensDto, pagingInfo);
        }

        /// <summary>
        /// Adiciona uma ordem à tabela do banco de dados.
        /// </summary>
        /// <param name="ordemDto">Objeto com os dados da ordem a ser adicionada.</param>
        /// <returns>Retorna a ordem adicionada.</returns>
        public async Task<OrdemDTO> Add(OrdemDTO ordemDto)
        {
            var ordemEntity = _mapper.Map<Ordem>(ordemDto);
            var ordem = await _ordemRepository.CreateAsync(ordemEntity);
            return _mapper.Map<OrdemDTO>(ordem);
        }

        /// <summary>
        /// Atualiza o status de uma Ordem.
        /// </summary>
        /// <param name="id">Id da Ordem.</param>
        /// <param name="statusVenda">Novo status.</param>
        /// <returns>Retorna a ordem atualizada.</returns>
        public async Task<OrdemDTO> UpdateStatus(int id, EnumStatusVenda statusVenda)
        {
            Ordem ordem = _ordemRepository.GetByIdAsync(x => x.Id == id).Result;

            ordem.UpdateStatus(statusVenda);

            var ordemEntity = _mapper.Map<Ordem>(ordem);
            await _ordemRepository.UpdateAsync(ordem);
            return _mapper.Map<OrdemDTO>(ordem);
        }

        /// <summary>
        /// Remove uma ordem do banco de dados.
        /// </summary>
        /// <param name="id">Id da ordem.</param>
        /// <returns></returns>
        public async Task Remove(int? id)
        {
            var ordemEntity = _ordemRepository.GetByIdAsync(x => x.Id == id).Result;
            await _ordemRepository.RemoveAsync(ordemEntity);
        }

        /// <summary>
        /// Converte a string da ordenação para uma expressão lambda.
        /// </summary>
        /// <param name="orderedBy">String com a palavra da ordenação.</param>
        /// <returns>Retorna a expressão lambda a ser usada para ordenar.</returns>
        private static Expression<Func<Ordem, object>> SwitchCaseOrderedBy(string orderedBy)
        {
            switch (orderedBy.ToLower())
            {
                case "vendedorid":
                case "sellerid":
                    return x => x.VendedorId;
                case "clienteid":
                case "clientid":
                    return x => x.ClienteId;
                case "total":
                    return x => x.Total;
                case "data":
                case "date":
                    return x => x.DataCriacao;
                default:
                    return x => x.Id;
            }
        }
    }
}