export const buildApiUrl = (path) => {
  return path.startsWith("/") ? path : `/${path}`;
};
